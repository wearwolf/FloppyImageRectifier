Public Class MfmTrackRevolution

    Public ReadOnly Property TrackData As BitList
    Public ReadOnly Property Sectors As List(Of MfmSector)
    Public ReadOnly Property RevolutionNumber As Integer
    Public Property RevolutionGroup As Integer
    Public ReadOnly Property SectorsValid As Boolean
    Public ReadOnly Property FixupsApplied As Boolean
    Public Property TimeEntries As List(Of Double)

    Public Sub New(trackData As BitList, sectors As List(Of MfmSector), revolutionNumber As Integer, fixupsApplied As Boolean)
        Me.TrackData = trackData
        Me.Sectors = sectors
        Me.RevolutionNumber = revolutionNumber
        Me.RevolutionGroup = revolutionNumber + 1
        Me.SectorsValid = sectors.All(Function(s) s.IdentChecksumValid AndAlso s.DataChecksumValid)
        Me.FixupsApplied = fixupsApplied
    End Sub

    Public Sub CheckRevolutionCheckSum(outputWriter As OutputWriter)
        For Each sector In Sectors
            If Not sector.IdentChecksumValid Then
                outputWriter.WriteLine($"Sector (C:{sector.TrackNumber}, H:{sector.SideNumber}, S:{sector.SectorNumber}) Identifier EDC doesn't match, found {sector.IdentCalculatedChecksum:X4}, should be {sector.IdentChecksum:X4}")
            End If

            If Not sector.DataChecksumValid Then
                outputWriter.WriteLine($"Sector (C:{sector.TrackNumber}, H:{sector.SideNumber}, S:{sector.SectorNumber}) Data EDC doesn't match, found {sector.DataCalculatedChecksum:X4}, should be {sector.DataChecksum:X4}")
            End If
        Next
    End Sub

    Public Function MatchPercentage(testRevolution As MfmTrackRevolution) As Double
        Return TrackData.MatchPercentage(testRevolution.TrackData)
    End Function

End Class
