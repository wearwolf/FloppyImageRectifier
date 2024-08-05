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

    Public Function GetRevolutions(side As Integer) As List(Of MfmTrackRevolution)
        If side = 0 Then
            Return Side0Revolutions
        ElseIf side = 1 Then
            Return Side1Revolutions
        End If

        Return Nothing
    End Function

    Public Sub SelectRevolution(index As Integer, side As Integer)
        If side = 0 Then
            If index < 0 OrElse index >= Side0Revolutions.Count Then
                Throw New InvalidOperationException($"Unable to select revolution {index} for side {side}, valid indexes are 0 to {Side0Revolutions.Count - 1}")
            End If

            m_side0SelectedRevolution = index
        ElseIf side = 1 Then
            If index < 0 OrElse index >= Side1Revolutions.Count Then
                Throw New InvalidOperationException($"Unable to select revolution {index} for side {side}, valid indexes are 0 to {Side1Revolutions.Count - 1}")
            End If

            m_side1SelectedRevolution = index
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

    Public Function IsValid() As Boolean
        Dim side0 = Side0Revolution
        If side0 IsNot Nothing AndAlso Not side0.IsValid() Then
            Return False
        End If

        Dim side1 = Side1Revolution
        If side1 IsNot Nothing AndAlso Not side1.IsValid() Then
            Return False
        End If

        Return True
    End Function
End Class
