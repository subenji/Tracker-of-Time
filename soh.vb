Imports System.Runtime.CompilerServices ' VB doesn't have preprocessor macros, so using Inlining - https://stackoverflow.com/a/37761883

Module soh
    Public Const gSaveCtxOff As Integer = &HEC8560
    Private gGameData As Long = 0

    <MethodImplAttribute(MethodImplOptions.AggressiveInlining)> 'This will instruct compiler to use aggressive inlining if possible. Should be just before the function definition
    Public Function SAV(offset As Integer) As Integer
        Return gSaveCtxOff + offset
    End Function

    Public Function GDATA(ByVal offset As Integer, Optional bytes As Byte = 4) As UInteger
        Select Case bytes
            Case 1
                Return ReadMemory(Of Byte)(gGameData + offset)
            Case 2
                Return ReadMemory(Of UInt16)(gGameData + offset)
            Case Else
                Return ReadMemory(Of UInteger)(gGameData + offset)
        End Select
    End Function

    Public Sub sohSetup(ByVal startAddress As Int64)
        gGameData = ReadMemory(Of Long)(startAddress + &HE4D878)

        ' Force some settings off until we can get to them
        My.Settings.setShop = 0
        frmTrackerOfTime.updateLTB("ltbShopsanity")
        My.Settings.setScrub = False
        My.Settings.setCow = False
        frmTrackerOfTime.updateSettingsPanel()


        ' Still need 61 - 73, 75, 78 - 99

        ' Check that we have not already done this, in case this is triggered twice in one instance
        If frmTrackerOfTime.arrLocation(0) = &H11AD1C Then ' original offset for emulators
            For i = 0 To frmTrackerOfTime.arrLocation.Length - 1
                ' Skip over 60 through 99
                If i = 60 Then i = 100
                frmTrackerOfTime.arrLocation(i) = frmTrackerOfTime.arrLocation(i) + &HDADF94 ' add additional offset from emu into soh RAM
            Next
        End If

        frmTrackerOfTime.arrLocation(74) = SAV(&H9E)
        frmTrackerOfTime.arrLocation(76) = SAV(&HA4)
        frmTrackerOfTime.arrLocation(77) = SAV(&HA8)
    End Sub
End Module
