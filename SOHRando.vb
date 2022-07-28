Partial Public Class frmTrackerOfTime
    Private sohGlobalCtx As Long = 0
    Private sohSaveCtx As Long = 0
    Private sohViewCtx As Long = 0

    Private Enum RandomizerSettingKey
        RSK_NONE
        RSK_FOREST
        RSK_KAK_GATE
        RSK_DOOR_OF_TIME
        RSK_ZORAS_FOUNTAIN
        RSK_GERUDO_FORTRESS
        RSK_RAINBOW_BRIDGE
        RSK_RAINBOW_BRIDGE_STONE_COUNT
        RSK_RAINBOW_BRIDGE_MEDALLION_COUNT
        RSK_RAINBOW_BRIDGE_REWARD_COUNT
        RSK_RAINBOW_BRIDGE_DUNGEON_COUNT
        RSK_RAINBOW_BRIDGE_TOKEN_COUNT
        RSK_RANDOM_TRIALS
        RSK_TRIAL_COUNT
        RSK_STARTING_OCARINA
        RSK_SHUFFLE_OCARINA
        RSK_STARTING_DEKU_SHIELD
        RSK_STARTING_KOKIRI_SWORD
        RSK_SHUFFLE_KOKIRI_SWORD
        RSK_STARTING_MAPS_COMPASSES 'RANDOTODO more options For this, rn it's just start with or own dungeon
        RSK_SHUFFLE_DUNGEON_REWARDS
        RSK_SHUFFLE_SONGS
        RSK_SHUFFLE_WEIRD_EGG
        RSK_SHUFFLE_GERUDO_TOKEN
        RSK_ITEM_POOL
        RSK_ICE_TRAPS
        RSK_GOSSIP_STONE_HINTS
        RSK_HINT_CLARITY
        RSK_HINT_DISTRIBUTION
        RSK_GANONS_BOSS_KEY
        RSK_SKIP_CHILD_ZELDA
        RSK_STARTING_CONSUMABLES
        RSK_EXCLUDE_DEKU_THEATER_MASK_OF_TRUTH
        RSK_LANGUAGE
        RSK_EXCLUDE_KAK_10_GOLD_SKULLTULA_REWARD
        RSK_EXCLUDE_KAK_20_GOLD_SKULLTULA_REWARD
        RSK_EXCLUDE_KAK_30_GOLD_SKULLTULA_REWARD
        RSK_EXCLUDE_KAK_40_GOLD_SKULLTULA_REWARD
        RSK_EXCLUDE_KAK_50_GOLD_SKULLTULA_REWARD
        RSK_SHUFFLE_CHEST_MINIGAME
    End Enum

    Private ReadOnly sohSaveContextDict As New Dictionary(Of Integer, Integer) From {
        {&H0, &H0}, ' Entrance Index
        {&H4, &H4}, ' link Age
        {&HA, &H8}, ' Cutscene Index
        {&HC, &HC}, ' World Time
        {&H10, &H10}, ' Night Flag
        {&H22, &H1C}, ' death count
        {&H24, &H1E}, ' Player name
        {&H2C, &H26}, ' DD Flag - Rando flag on soh
        {&H2E, &H28}, ' Heart Containers
        {&H30, &H2A}, ' current health
        {&H32, &H2C}, ' magic meter capacity
        {&H33, &H2D}, ' current magic
        {&H34, &H2E},  ' rupees
        {&H11A5D4, &H4}, ' Link age
        {&H11A5EC, &H0}, ' ZELDAZ - not present in soh
        {&H11A5F2, &H0},  ' ZELDAZ upper word - not present in soh
        {&H11A5F4, &H1E}, ' Player name
        {&H11A5F8, &H22}, ' Player name
        {&H11A5FC, &H28}, ' Heart Containers
        {&H11A60E, &H36}, ' Half Damage
        {&H11B92C, &H1320} ' Game play state        
    }
    '{&H11A677, &HA6}, ' Pieces of Heart
    '{&H11A6A2, &HD4}, ' gold skulltula tokens

    Private ReadOnly sohExpansionDict As New Dictionary(Of Integer, Integer) From {
        {&H400008, &H0} ' rando version check
    }

    Private Sub AttachToSoH()
        emulator = String.Empty
        If IS_64BIT = False Then Exit Sub

        Dim gSaveContext As IntPtr
        Dim gGlobalContext As IntPtr
        Dim target As Process

        Try
            ' Try to attach to application
            target = Process.GetProcessesByName("soh")(0)
        Catch ex As Exception
            If ex.Message = "Index was outside the bounds of the array." Then
                ' This is the expected error if process was not found, just return
                Return
            Else
                ' Any other error, output error message to textbox
                rtbOutputLeft.Text = "Attachment Problem: " & ex.Message & vbCrLf
                Return
            End If
        End Try

        Dim AOB As New dotNetMemoryScan()
        Dim entryPoint As IntPtr = AOB.scan_module(target, "soh.exe", "00 00 00 00 57 45 49 56") + 4 '"....WEIV" - Start Position Of View Context
        Console.WriteLine($"Ptr: {entryPoint.ToString("X16")}")
        If entryPoint = IntPtr.Zero Then
            rtbOutputLeft.Text = "Could not find entry point for SOH" & vbCrLf
            Return
        End If

        ' Attach to process and set it as the current emulator
        SetProcessName("soh")
        Dim outR15 As Integer = ReadMemory(Of Integer)(entryPoint)
        If outR15 = &H56494557 Then
            Console.WriteLine("Found VIEW from AOB!" & vbCrLf)

            ' SaveContext
            Dim ep As IntPtr = AOB.scan_module(target, "soh.exe", "41 8B DE 4C 8D 3D ?? ?? ?? ?? 0F 1F 40 00") + 6
            If ep <> IntPtr.Zero Then
                Dim offset = ReadMemory(Of Integer)(ep)
                Console.WriteLine($"gSaveContextOffset: 0x{Hex(offset)}")
                gSaveContext = IntPtr.Add(ep + 4, offset)
                Console.WriteLine($"gSaveContext test: gSaveContext: 0x{Hex(gSaveContext.ToInt64)}")
            Else
                Console.WriteLine("gSaveContext not found!")
                Return
            End If

            ' GlobalContext
            Dim ep2 = AOB.scan_module(target, "soh.exe", "4C 8B 35 ?? ?? ?? ?? 4D 85 F6 75 61") + 3
            If ep2 <> IntPtr.Zero Then
                Dim offset = ReadMemory(Of Integer)(ep2)
                gGlobalContext = IntPtr.Add(ep2 + 4, offset)
                Console.WriteLine($"gGlobalContext test: gGameContext: 0x{Hex(gGlobalContext.ToInt64)}")
            Else
                Console.WriteLine("gGlobalContext not found!")
                Return
            End If


            ' We got what we need, set globals
            romAddrStart64 = gSaveContext.ToInt64
            sohViewCtx = entryPoint.ToInt64
            sohSaveCtx = gSaveContext.ToInt64
            sohGlobalCtx = gGlobalContext.ToInt64
            emulator = "soh"
        End If
    End Sub

    Private Sub TranslateOffsetForSOH(ByRef offset As Integer)
        ' get relevant memory area we're in
        Select Case offset
            Case 0 To &H11A5CF ' ROM area
                Console.WriteLine($"goRead attempted to read ROM area {Hex(offset)} - unhandled")
                offset = 0
            Case &H11A5D0 To &H11D4FF ' Save Context
                Dim saveDataOffset = &H11A5D0
                Dim x = offset - saveDataOffset
                Dim y As Integer = 0
                Select Case x
                    Case &HD2
                        y = x + 2 ' gs tokens
                        Exit Select ' overlaps with below check
                    Case &H74 To &HE63 ' inventory, inv ammo, beans, equip, upgrades, quest items, dungeon items, small keys, dd counter, Permanent Scene Flags
                        y = x + 4
                    Case Else
                        If Not sohSaveContextDict.TryGetValue(offset, y) Then
                            Console.WriteLine($"goRead attempted to read SaveData: {Hex(offset)} - Translating to {Hex(x)}, returning {Hex(y)}")
                        End If
                End Select
                offset = y
            Case &H1C84A0 To &H1DA9EF ' game_play - global context
                Dim gamePlayOffset = &H1C84A0
                Console.WriteLine($"goRead attempted to read game_play state: {Hex(offset - gamePlayOffset)} - unhandled")
                offset = 0 'todo
            Case &H1DA9F0 To &H3FFFFF ' Higher RAM
                Console.WriteLine($"goRead attempted to read higher RAM area: {Hex(offset)} - unhandled")
                offset = 0
            Case Is >= &H400000 ' Beyond normal 4MB RAM, Rando data
                Dim y As Integer = 0
                sohExpansionDict.TryGetValue(offset, y)
                Console.WriteLine($"goRead attempted to read Epansion RAM area: {Hex(offset)}")
                offset = -(romAddrStart64 - sohViewCtx)
        End Select
    End Sub
End Class