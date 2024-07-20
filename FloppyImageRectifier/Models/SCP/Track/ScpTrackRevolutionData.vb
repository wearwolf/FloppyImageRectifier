Imports System.IO

Public Class ScpTrackRevolutionData
    Public Property TrackOffset As ScpTrackOffset
    Public Property RevolutionHeader As ScpTrackRevolutionHeader
    Public Property TimeEntries As List(Of UShort)

    Public Sub New(trackOffset As ScpTrackOffset, revolutionHeader As ScpTrackRevolutionHeader)
        Me.TrackOffset = trackOffset
        Me.RevolutionHeader = revolutionHeader
    End Sub

    Public Sub Read(fstream As FileStream, binReader As BinaryReader)
        fstream.Seek(TrackOffset.Offset + RevolutionHeader.Offset, SeekOrigin.Begin)
        TimeEntries = New List(Of UShort)()
        For i = 0 To RevolutionHeader.Length - 1
            Dim bytes = binReader.ReadBytes(2)
            Dim value = (CType(bytes(0), UShort) << 8) + bytes(1)
            TimeEntries.Add(value)
        Next
    End Sub

    Public Sub WriteOutput(outputWriter As OutputWriter)
        outputWriter.Write($"Revolution Number = {RevolutionHeader.RevolutionNumber}")
        For i = 0 To TimeEntries.Count - 1
            If i Mod 20 = 0 Then
                outputWriter.WriteLine()
            End If
            outputWriter.Write($"{TimeEntries(i)},")
        Next
        outputWriter.WriteLine()
    End Sub
End Class
