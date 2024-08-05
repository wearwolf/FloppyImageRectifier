Public Class MfmTrackRevolution

    Public ReadOnly Property TrackData As BitList
    Public ReadOnly Property Sectors As List(Of MfmSector)

    Public Sub New(trackData As BitList, sectors As List(Of MfmSector))
        Me.TrackData = trackData
        Me.Sectors = sectors
    End Sub

    Public Sub CheckRevolutionCheckSum(outputWriter As OutputWriter)
        For Each sector In Sectors
            If Not sector.SectorIdentifierChecksumValid Then
                outputWriter.WriteLine($"Sector (C:{sector.TrackNumber}, H:{sector.SideNumber}, S:{sector.SectorNumber}) Identifier EDC doesn't match, found {sector.SectorIdentifierCalculatedChecksum:X4}, should be {sector.SectorIdentifierChecksum:X4}")
            End If

            If Not sector.DataChecksumValid Then
                outputWriter.WriteLine($"Sector (C:{sector.TrackNumber}, H:{sector.SideNumber}, S:{sector.SectorNumber}) Data EDC doesn't match, found {sector.DataCalculatedChecksum:X4}, should be {sector.DataChecksum:X4}")
            End If
        Next
    End Sub

    Public Function IsValid() As Boolean
        For Each sector In Sectors
            If Not sector.SectorIdentifierChecksumValid Then
                Return False
            End If

            If Not sector.DataChecksumValid Then
                Return False
            End If
        Next

        Return True
    End Function

    Public Function MatchPercentage(testRevolution As MfmTrackRevolution) As Double
        Return TrackData.MatchPercentage(testRevolution.TrackData)
    End Function

End Class
