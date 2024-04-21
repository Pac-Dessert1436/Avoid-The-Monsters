Imports Godot

Namespace AvoidTheMonsters
    Public MustInherit Class PlayerVB
        Inherits Area2D

        <Export> Public ReadOnly speed As Integer = 400
        Friend screenSize As Vector2
        Private velocity As Vector2

        Public Overrides Sub _Ready()
            Hide()
            screenSize = GetViewportRect().Size
        End Sub

        Public Overrides Sub _Process(delta As Single)
            velocity = Vector2.Zero
            Dim playerAnim = GetNode(Of AnimatedSprite)("AnimatedSprite")
            If Not Input.IsActionPressed("finger_down") Then
                If Input.IsActionPressed("move_left") Then velocity.x -= 1
                If Input.IsActionPressed("move_right") Then velocity.x += 1
                If Input.IsActionPressed("move_up") Then velocity.y -= 1
                If Input.IsActionPressed("move_down") Then velocity.y += 1
                If velocity.Length() > 0 Then
                    velocity = velocity.Normalized() * speed
                    playerAnim.Play()
                Else
                    playerAnim.Stop()
                End If
                Position += velocity * delta
            ElseIf Input.IsActionPressed("finger_down") Then
                Dim target As Vector2 = GetGlobalMousePosition()
                velocity = Position.DirectionTo(target) * speed
                If Position.DistanceTo(target) > 5 Then
                    Position += velocity * delta
                End If
            End If
            Dim x1 As Single = Mathf.Clamp(Position.x, 0, screenSize.x)
            Dim y1 As Single = Mathf.Clamp(Position.y, 0, screenSize.y)
            Position = New Vector2(x1, y1)
            If velocity.x <> 0 Then
                playerAnim.Animation = "walk"
                playerAnim.FlipV = False
                playerAnim.FlipH = velocity.x < 0
            ElseIf velocity.y <> 0 Then
                playerAnim.Animation = "up"
                playerAnim.FlipV = velocity.y > 0
            End If
        End Sub

        <Signal> Public Delegate Sub Hit()

        Public Sub OnPlayerBodyEntered(body As PhysicsBody2D)
            Hide()
            EmitSignal(NameOf(Hit))
            GetNode(Of CollisionShape2D)("CollisionShape2D").SetDeferred("disabled", True)
        End Sub

        Public Sub Start(pos As Vector2)
            Position = pos
            Show()
            GetNode(Of CollisionShape2D)("CollisionShape2D").Disabled = False
        End Sub
    End Class

    Public MustInherit Class EnemyVB
        Inherits RigidBody2D

        Public Overrides Sub _Ready()
            Dim enemyAnim = GetNode(Of AnimatedSprite)("AnimatedSprite")
            enemyAnim.Playing = True
            Dim enemyTypes As String() = enemyAnim.Frames.GetAnimationNames()
            Dim idx As Integer = GD.Randi() Mod enemyTypes.Length
            enemyAnim.Animation = enemyTypes(idx)
        End Sub

        Public Sub OnVisibilityNotifier2DScreenExited()
            QueueFree()
        End Sub
    End Class

    Public MustInherit Class MainSceneVB
        Inherits Node

        <Export> Public enemyScene As PackedScene
        Public score As Integer

        Public Overrides Sub _Ready()
            GD.Randomize()
        End Sub

        Public Sub GameOver()
            GetNode(Of Timer)("EnemyTimer").Stop()
            GetNode(Of Timer)("ScoreTimer").Stop()
            GetNode(Of HeadUpDisplayVB)("HUD").ShowGameOver()
            GetNode(Of AudioStreamPlayer)("MainTheme").Stop()
            GetNode(Of AudioStreamPlayer)("EndingSound").Play()
        End Sub

        Public Sub NewGame()
            GetTree().CallGroup("enemies", "queue_free")
            score = 0
            Dim player = GetNode(Of PlayerVB)("Player")
            Dim startPosition = GetNode(Of Position2D)("StartPosition")
            player.Start(startPosition.Position)
            GetNode(Of Timer)("StartTimer").Start()
            Dim hud = GetNode(Of HeadUpDisplayVB)("HUD")
            hud.UpdateScore(score)
            hud.ShowMessage("Get Ready!")
            GetNode(Of AudioStreamPlayer)("MainTheme").Play()
        End Sub

        Public Sub OnScoreTimerTimeout()
            score += 1
            GetNode(Of HeadUpDisplayVB)("HUD").UpdateScore(score)
        End Sub

        Public Sub OnStartTimerTimeout()
            GetNode(Of Timer)("EnemyTimer").Start()
            GetNode(Of Timer)("ScoreTimer").Start()
        End Sub

        Public Sub OnEnemyTimerTimeout()
            Dim enemy As EnemyVB = CType(enemyScene.Instance(), EnemyVB)
            Dim spawnPoints = GetNode(Of PathFollow2D)("EnemyPath/EnemySpawnPoints")
            spawnPoints.Offset = GD.Randi()
            Dim direction As Single = spawnPoints.Rotation + Mathf.Pi / 2
            enemy.Position = spawnPoints.Position
            direction += CSng(GD.RandRange(-Mathf.Pi / 4, Mathf.Pi / 4))
            enemy.Rotation = direction
            Dim velocity As New Vector2(GD.RandRange(150.0, 250.0), 0)
            enemy.LinearVelocity = velocity.Rotated(direction)
            AddChild(enemy)
        End Sub
    End Class

    Public MustInherit Class HeadUpDisplayVB
        Inherits CanvasLayer

        <Signal> Public Delegate Sub StartGame()

        Public Sub ShowMessage(text As String)
            Dim message = GetNode(Of Label)("Message")
            message.Text = text
            message.Show()
            GetNode(Of Timer)("MessageTimer").Start()
        End Sub

        Public MustOverride Sub ShowGameOver()

        Public Sub UpdateScore(score As Integer)
            GetNode(Of Label)("ScoreLabel").Text = score.ToString()
        End Sub

        Public Sub OnStartButtonPressed()
            GetNode(Of Button)("StartButton").Hide()
            EmitSignal(NameOf(StartGame))
        End Sub

        Public Sub OnMessageTimerTimeout()
            GetNode(Of Label)("Message").Hide()
        End Sub
    End Class
End Namespace