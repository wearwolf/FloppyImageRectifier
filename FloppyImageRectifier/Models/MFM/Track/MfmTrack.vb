Public Class MfmTrack

    Private m_side0SelectedRevolution = 0
    Private m_side1SelectedRevolution = 0

    Public ReadOnly Property TrackNumber As Integer
    Public ReadOnly Property Side0Revolutions As List(Of MfmTrackRevolution)
    Public ReadOnly Property Side1Revolutions As List(Of MfmTrackRevolution)

    Public ReadOnly Property Side0Revolution As MfmTrackRevolution
        Get
            If Side0Revolutions.Any() Then
                Return Side0Revolutions(m_side0SelectedRevolution)
            End If
            Return Nothing
        End Get
    End Property

    Public ReadOnly Property Side1Revolution As MfmTrackRevolution
        Get
            If Side1Revolutions.Any() Then
                Return Side1Revolutions(m_side1SelectedRevolution)
            End If
            Return Nothing
        End Get
    End Property

    Public Sub New(trackNumber As Integer)
        Me.TrackNumber = trackNumber
        Side0Revolutions = New List(Of MfmTrackRevolution)
        Side1Revolutions = New List(Of MfmTrackRevolution)
    End Sub

    Public Sub AddRevolution(revolution As MfmTrackRevolution, side As Integer)
        If side = 0 Then
            Side0Revolutions.Add(revolution)
        ElseIf side = 1 Then
            Side1Revolutions.Add(revolution)
        End If
    End Sub

    Public Sub CheckChecksums(outputWriter As OutputWriter)
        Dim side0 = Side0Revolution
        If side0 IsNot Nothing Then
            side0.CheckRevolutionCheckSum(outputWriter)
        End If

        Dim side1 = Side1Revolution
        If side1 IsNot Nothing Then
            side1.CheckRevolutionCheckSum(outputWriter)
        End If
    End Sub
End Class
