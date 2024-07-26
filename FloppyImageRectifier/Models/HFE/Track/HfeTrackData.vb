Imports System.IO

Public Class HfeTrackData
    Private Const SIDE_BLOCK_LENGTH = 256

    Public Property TrackOffset As HfeTrackOffset
    Public Property Side0 As List(Of Byte)
    Public Property Side1 As List(Of Byte)

    Public Sub New(trackOffset As HfeTrackOffset)
        Me.TrackOffset = trackOffset
    End Sub

    Public Sub Read(fstream As FileStream, binReader As BinaryReader)
        Dim startOfTrack = TrackOffset.Offset * HfeFile.BLOCK_LENGTH
        Dim endOfTrack = startOfTrack + TrackOffset.TrackLength

        fstream.Seek(startOfTrack, SeekOrigin.Begin)
        Dim side0TrackBytes = New List(Of Byte)
        Dim side1TrackBytes = New List(Of Byte)

        Dim pos = startOfTrack
        While pos < endOfTrack
            Dim side0Bytes = binReader.ReadBytes(SIDE_BLOCK_LENGTH)
            side0TrackBytes.AddRange(side0Bytes)

            Dim side1Bytes = binReader.ReadBytes(SIDE_BLOCK_LENGTH)
            side1TrackBytes.AddRange(side1Bytes)

            pos += HfeFile.BLOCK_LENGTH
        End While

        Side0 = side0TrackBytes
        Side1 = side1TrackBytes
    End Sub

    Public Sub Write(fstream As FileStream, binWriter As BinaryWriter)
        fstream.Seek(HfeFile.BLOCK_LENGTH * TrackOffset.Offset, SeekOrigin.Begin)
        Dim data = New List(Of Byte)()
        Dim sideLength = Math.Min(Side0.Count, Side1.Count)
        Dim trackLength = sideLength * 2

        Dim index = 0
        While index < sideLength
            Dim side0Block As List(Of Byte)
            Dim side1Block As List(Of Byte)
            Dim byteRemaining = sideLength - index
            If byteRemaining < SIDE_BLOCK_LENGTH Then
                side0Block = Side0.GetRange(index, byteRemaining)
                side1Block = Side1.GetRange(index, byteRemaining)

                For i = 1 To SIDE_BLOCK_LENGTH - byteRemaining
                    side0Block.Add(0)
                    side1Block.Add(0)
                Next
            Else
                side0Block = Side0.GetRange(index, SIDE_BLOCK_LENGTH)
                side1Block = Side1.GetRange(index, SIDE_BLOCK_LENGTH)
            End If
            index += SIDE_BLOCK_LENGTH

            data.AddRange(side0Block)
            data.AddRange(side1Block)
        End While

        binWriter.Write(data.ToArray())
    End Sub

    Public Sub WriteOutput(outputWriter As OutputWriter)
        outputWriter.WriteLine($"Track Number = {TrackOffset.TrackNumber}")
        outputWriter.WriteLine("Side 0")
        outputWriter.PrintBytes(Side0, 0, 0, OutputWriter.BytePrinterFormat.BinaryLittleEndian)

        outputWriter.WriteLine("Side 1")
        outputWriter.PrintBytes(Side1, 0, 0, OutputWriter.BytePrinterFormat.BinaryLittleEndian)
    End Sub
End Class
