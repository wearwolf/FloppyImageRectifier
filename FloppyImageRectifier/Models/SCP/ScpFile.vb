Imports System.IO

Public Class ScpFile
    Private Const MAX_TRACK_NUMBER = 167
    Private Const FOOTER_LENGTH = 48

    Public Property FilePath As String
    Public Property Header As ScpHeader
    Public Property TrackOffsets As List(Of ScpTrackOffset)
    Public Property Tracks As List(Of ScpTrack)
    Public Property Footer As ScpFooter

    Public Sub New(filePath As String)
        Me.FilePath = filePath
        TrackOffsets = New List(Of ScpTrackOffset)
        Tracks = New List(Of ScpTrack)
    End Sub

    Public Sub Read()
        Using fstream = File.OpenRead(FilePath)
            Using binReader = New BinaryReader(fstream)
                Header = New ScpHeader()
                Header.Read(binReader)

                For index = 0 To MAX_TRACK_NUMBER
                    Dim trackDataHeaderOffset = New ScpTrackOffset()
                    trackDataHeaderOffset.Read(binReader, index)

                    TrackOffsets.Add(trackDataHeaderOffset)
                Next

                For Each trackOffset In TrackOffsets
                    If trackOffset.TrackNumber < Header.StartTrack OrElse
                            trackOffset.TrackNumber > Header.EndTrack OrElse
                            trackOffset.Offset = 0 Then
                        Continue For
                    End If

                    Dim track = New ScpTrack(trackOffset)
                    track.Read(fstream, binReader, Header.NumberOfRevolutionsPerTrack)

                    Tracks.Add(track)
                Next

                Footer = New ScpFooter()
                Footer.Read(fstream, binReader)
            End Using
        End Using
    End Sub

    Public Sub Write()
        Using fstream = File.OpenWrite(FilePath)
            Using BinaryWriter = New BinaryWriter(fstream)
                ' Only the header is written because that's the only part that should be changed
                Header.Write(BinaryWriter)
            End Using
        End Using
    End Sub

    Public Sub WriteOutput(outputWriter As OutputWriter)
        Header.WriteOutput(outputWriter)
        outputWriter.WriteLine()

        For Each trackOffset In TrackOffsets
            trackOffset.WriteOutput(outputWriter)
            outputWriter.WriteLine()
        Next

        For Each track In Tracks
            track.WriteOutput(outputWriter)
        Next

        Footer.WriteOutput(outputWriter)
    End Sub

End Class
