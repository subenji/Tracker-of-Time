Module soh
    Public Const gSaveCtxOff As Integer = &HEC8560

    Public Function SAV(offset As Integer) As Integer
        Return gSaveCtxOff + offset
    End Function

    Public Sub sohSetup()
        frmTrackerOfTime.arrLocation(74) = SAV(&H9E)
        frmTrackerOfTime.arrLocation(77) = SAV(&HA8)
    End Sub
End Module
