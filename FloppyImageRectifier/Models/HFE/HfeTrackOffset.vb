Imports System.IO

Public Class HfeTrackOffset
    Public Property TrackNumber As UShort
    Public Property Offset As UShort
    Public Property TrackLength As UShort

    Public Sub Read(binReader As BinaryReader, trackNumber As UShort)
        Me.TrackNumber = trackNumber
        Offset = BitConverter.ToUInt16(binReader.ReadBytes(2))
        TrackLength = BitConverter.ToUInt16(binReader.ReadBytes(2))
    End Sub

    Public Sub Write(binWriter As BinaryWriter)
        binWriter.Write(Offset)
        binWriter.Write(TrackLength)
    End Sub

    Public Sub WriteOutput(outputWriter As OutputWriter)
        outputWriter.WriteLine($"Track Number = {TrackNumber}")
        outputWriter.WriteLine($"offset = {Offset}")
        outputWriter.WriteLine($"track_len = {TrackLength}")
    End Sub
End Class
