Imports System.IO
Imports System.Text

Public Class HfeFile
    Public Const BLOCK_LENGTH = 512
    Public Property FilePath As String

    Public Property Header As HfeHeader
    Public Property TrackOffsets As List(Of HfeTrackOffset)
    Public Property Tracks As List(Of HfeTrackData)

    Public Sub New(filePath As String)
        Me.FilePath = filePath
        TrackOffsets = New List(Of HfeTrackOffset)
        Tracks = New List(Of HfeTrackData)
    End Sub

    Public Sub Read()
        Using fstream = File.OpenRead(FilePath)
            Using binReader = New BinaryReader(fstream, Encoding.Latin1)
                Header = New HfeHeader()
                Header.Read(binReader)

                fstream.Seek(Header.TrackListOffset * 512, SeekOrigin.Begin)
                For index = 0 To Header.NumberOfTracks - 1
                    Dim trackOffset = New HfeTrackOffset()
                    trackOffset.Read(binReader, index)
                    TrackOffsets.Add(trackOffset)
                Next

                For Each trackOffset In TrackOffsets
                    Dim trackData = New HfeTrackData(trackOffset)
                    trackData.Read(fstream, binReader)
                    Tracks.Add(trackData)
                Next
            End Using
        End Using
    End Sub

    Public Sub Write()
        Using fstream = File.Create(FilePath)
            Using binWriter = New BinaryWriter(fstream, Encoding.Latin1)
                Header.Write(binWriter)

                fstream.Seek(BLOCK_LENGTH, SeekOrigin.Begin)
                For Each trackOffset In TrackOffsets
                    trackOffset.Write(binWriter)
                Next

                For Each trackData In Tracks
                    trackData.Write(fstream, binWriter)
                Next
            End Using
        End Using
    End Sub

    Public Sub WriteOutput(outputWriter As OutputWriter)
        Header.WriteOutput(outputWriter)

        For Each trackOffset In TrackOffsets
            trackOffset.WriteOutput(outputWriter)
        Next

        For Each TrackData In Tracks
            TrackData.WriteOutput(outputWriter)
        Next
    End Sub

End Class
