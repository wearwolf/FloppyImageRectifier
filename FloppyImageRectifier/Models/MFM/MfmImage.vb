Public Class MfmImage

    Public ReadOnly Property DiskType As FloppyDiskType
    Public ReadOnly Property Tracks As List(Of MfmTrack)

    Public Sub New(diskType As FloppyDiskType)
        Me.DiskType = diskType
        Tracks = New List(Of MfmTrack)
    End Sub

    Public Sub AddTrack(track As MfmTrack)
        Tracks.Add(track)
    End Sub

    Public Sub CheckChecksums(outputWriter As OutputWriter)
        For Each track In Tracks
            track.CheckChecksums(outputWriter)
        Next
    End Sub
End Class
