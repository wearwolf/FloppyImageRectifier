Imports System.IO

Public Class ScpTrack
    Public Property TrackOffset As ScpTrackOffset
    Public Property TrackDataHeader As ScpTrackDataHeader
    Public Property TrackRevolutionHeaders As List(Of ScpTrackRevolutionHeader)
    Public Property TrackRevolutionData As List(Of ScpTrackRevolutionData)

    Public Sub New(trackOffset As ScpTrackOffset)
        Me.TrackOffset = trackOffset
        TrackRevolutionHeaders = New List(Of ScpTrackRevolutionHeader)()
        TrackRevolutionData = New List(Of ScpTrackRevolutionData)()
    End Sub

    Public Sub Read(fstream As FileStream, binReader As BinaryReader, numberOfRevolutions As Byte)
        fstream.Seek(TrackOffset.Offset, SeekOrigin.Begin)
        TrackDataHeader = New ScpTrackDataHeader()
        TrackDataHeader.Read(binReader)

        For i = 0 To numberOfRevolutions - 1
            Dim trackRevolutionHeader = New ScpTrackRevolutionHeader(i)
            trackRevolutionHeader.Read(binReader)

            TrackRevolutionHeaders.Add(trackRevolutionHeader)
        Next

        For Each revolutionHeader In TrackRevolutionHeaders
            Dim trackRevolutionData = New ScpTrackRevolutionData(TrackOffset, revolutionHeader)
            trackRevolutionData.Read(fstream, binReader)

            Me.TrackRevolutionData.Add(trackRevolutionData)
        Next
    End Sub

    Public Sub WriteOutput(outputWriter As OutputWriter)
        TrackDataHeader.WriteOutput(outputWriter)
        outputWriter.WriteLine()

        For Each revolutionHeader In TrackRevolutionHeaders
            revolutionHeader.WriteOutput(outputWriter)
            outputWriter.WriteLine()
        Next

        For Each revolutionData In TrackRevolutionData
            revolutionData.WriteOutput(outputWriter)
            outputWriter.WriteLine()
        Next
    End Sub
End Class
