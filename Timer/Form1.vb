' Timer⏳

' This application is designed to provide an intuitive and user-friendly
' countdown timer experience.

' Features

'  Countdown Timer: Easily set and track countdowns for various activities.

'  Pause/Resume Functionality: Take control of your timing with the ability
'  to pause and resume as needed.

'  Customizable Duration: Set your desired timer duration with ease.

'  Sound Effects: Enjoy auditory alerts with integrated sound effects when
'  the timer completes.

'  Responsive UI: The graphical interface adapts seamlessly to different window
'  sizes, ensuring a consistent user experience across devices.

' MIT License
' Copyright(c) 2024 Joseph W. Lumbley

' Permission Is hereby granted, free Of charge, to any person obtaining a copy
' of this software And associated documentation files (the "Software"), to deal
' in the Software without restriction, including without limitation the rights
' to use, copy, modify, merge, publish, distribute, sublicense, And/Or sell
' copies of the Software, And to permit persons to whom the Software Is
' furnished to do so, subject to the following conditions:

' The above copyright notice And this permission notice shall be included In all
' copies Or substantial portions of the Software.

' THE SOFTWARE Is PROVIDED "AS IS", WITHOUT WARRANTY Of ANY KIND, EXPRESS Or
' IMPLIED, INCLUDING BUT Not LIMITED To THE WARRANTIES Of MERCHANTABILITY,
' FITNESS FOR A PARTICULAR PURPOSE And NONINFRINGEMENT. IN NO EVENT SHALL THE
' AUTHORS Or COPYRIGHT HOLDERS BE LIABLE For ANY CLAIM, DAMAGES Or OTHER
' LIABILITY, WHETHER In AN ACTION Of CONTRACT, TORT Or OTHERWISE, ARISING FROM,
' OUT OF Or IN CONNECTION WITH THE SOFTWARE Or THE USE Or OTHER DEALINGS IN THE
' SOFTWARE.

' https://github.com/JoeLumbley/Timer

Imports System.ComponentModel
Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text

Public Class Form1

    Public Enum AppState
        Initial
        Running
        Paused
        Stopped
        Completed
    End Enum

    Private TimerState As AppState = AppState.Initial

    Private StartTime As DateTime = Now

    Private Duration As New TimeSpan(0, 0, 10)

    Private InitialEntry As String = String.Empty

    <DllImport("winmm.dll", EntryPoint:="mciSendStringW")>
    Private Shared Function mciSendStringW(<MarshalAs(UnmanagedType.LPTStr)> ByVal lpszCommand As String,
                                           <MarshalAs(UnmanagedType.LPWStr)> ByVal lpszReturnString As StringBuilder,
                                           ByVal cchReturn As UInteger, ByVal hwndCallback As IntPtr) As Integer
    End Function

    Private Sounds() As String


    Private Context As BufferedGraphicsContext
    Private Buffer As BufferedGraphics

    Private Structure DisplayObject
        Public Location As Point
        Public Text As String
        Public Font As Font
    End Structure

    Private Structure ButtonStruct
        Public Rect As Rectangle
        Public Radius As Integer
        Public Text As String
        Public TextLocation As Point
        Public Font As Font
    End Structure

    Private MainDisplay As DisplayObject

    Private TopDisplay As DisplayObject

    Private BottomDisplay As DisplayObject

    Private InitialDisplay As DisplayObject


    Private StatusDisplay As DisplayObject





    Private CircleOfProgress As Rectangle

    Private CircleOfProgressPen As Pen

    Private CircleOfProgressBackgroundPen As Pen

    Private RatioDegDuration As Single

    ' Define the start angle in degrees at -90 or 12 o'clock.
    Private Const startAngle As Single = -90.0F

    Private sweepAngle As Single

    Private StopButton As ButtonStruct

    Private StartButton As ButtonStruct

    Private RestartButton As ButtonStruct

    Private PauseButton As ButtonStruct

    Private ResumeButton As ButtonStruct

    Private ElapsedTime As TimeSpan

    Private ReadOnly AlineCenterMiddle As New StringFormat With {.Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center}

    Private ReadOnly EmojiFont As New Font("Segoe UI Emoji", 25)

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        InitializeForm()

        InitializeBuffer()

        CreateSoundFileFromResource()

        AddSound("timesup", $"{Application.StartupPath}timesup.mp3")

        SetVolume("timesup", 500)

        StopButton.Text = "■"

        StartButton.Text = "▶"

        RestartButton.Text = "▶"

        PauseButton.Text = "⏸"

        ResumeButton.Text = "▶"

        StatusDisplay.Text = "Initial"

        Timer1.Interval = 15

        Timer1.Enabled = True

        Debug.Print($"Program running... {Now.ToShortTimeString}")

    End Sub

    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles Me.Resize

        If Not WindowState = FormWindowState.Minimized Then

            ResizeMainDisplay()

            ResizeInitialDisplay()

            ResizeRestartButton()

            ResizeStopButton()

            ResizeStartButton()

            ResizePauseButton()

            ResizeResumeButton()

            ResizeCircleOfProgress()

            ResizeStatusDisplay()

            DisposeBuffer()

        End If

    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick

        UpdateDisplays()

        If Not WindowState = FormWindowState.Minimized Then

            Refresh() ' Calls OnPaint Sub

        End If

        UpdateSound()

    End Sub

    Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)

        AllocateBuffer(e)

        DrawDisplays()

        Buffer.Render(e.Graphics)

    End Sub

    Private Sub Form1_MouseDown(sender As Object, e As MouseEventArgs) Handles Me.MouseDown

        Select Case TimerState

            Case AppState.Completed

                If StopButton.Rect.Contains(e.Location) Then

                    TimerState = AppState.Stopped

                    StatusDisplay.Text = "Stopped"

                End If

            Case AppState.Initial

                If StartButton.Rect.Contains(e.Location) Then

                    StartTimer()

                End If

            Case AppState.Stopped

                If RestartButton.Rect.Contains(e.Location) Then

                    TimerState = AppState.Running

                    StatusDisplay.Text = "Running"

                    StartTime = Now

                End If

            Case AppState.Running

                If PauseButton.Rect.Contains(e.Location) Then

                    TogglePause()

                End If

            Case AppState.Paused

                If ResumeButton.Rect.Contains(e.Location) Then

                    TogglePause()

                End If

        End Select

    End Sub

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown

        Select Case e.KeyValue

            Case Keys.Back

                ReturnToInitialEntryScreen()

                DeleteLastInitialEntryCharacter()

            Case Keys.Delete

                ReturnToInitialEntryScreen()

                DeleteLastInitialEntryCharacter()

            Case Keys.X

                ReturnToInitialEntryScreen()

                DeleteLastInitialEntryCharacter()

            Case Keys.Escape

                Select Case TimerState

                    Case AppState.Stopped

                        TimerState = AppState.Initial

                    Case AppState.Paused

                        TimerState = AppState.Initial

                    Case AppState.Running

                        TimerState = AppState.Initial

                    Case AppState.Initial

                        DeleteLastInitialEntryCharacter()

                End Select

            Case Keys.Pause

                TogglePause()

            Case Keys.P

                TogglePause()

            Case Keys.D0

                ReturnToInitialEntryScreen()

                If TimerState = AppState.Initial Then

                    If InitialEntry.Length < 6 Then

                        If Not InitialEntry = String.Empty Then

                            InitialEntry = InitialEntry & "0"

                        End If

                    End If

                End If

            Case Keys.D1

                ReturnToInitialEntryScreen()

                If TimerState = AppState.Initial Then

                    If InitialEntry.Length < 6 Then

                        InitialEntry = InitialEntry & "1"

                    End If

                End If

            Case Keys.D2

                ReturnToInitialEntryScreen()

                If TimerState = AppState.Initial Then

                    If InitialEntry.Length < 6 Then

                        InitialEntry = InitialEntry & "2"

                    End If

                End If

            Case Keys.D3

                ReturnToInitialEntryScreen()

                If TimerState = AppState.Initial Then

                    If InitialEntry.Length < 6 Then

                        InitialEntry = InitialEntry & "3"

                    End If

                End If

            Case Keys.D4

                ReturnToInitialEntryScreen()

                If TimerState = AppState.Initial Then

                    If InitialEntry.Length < 6 Then

                        InitialEntry = InitialEntry & "4"

                    End If

                End If

            Case Keys.D5

                ReturnToInitialEntryScreen()

                If TimerState = AppState.Initial Then

                    If InitialEntry.Length < 6 Then

                        InitialEntry = InitialEntry & "5"

                    End If

                End If

            Case Keys.D6

                ReturnToInitialEntryScreen()

                If TimerState = AppState.Initial Then

                    If InitialEntry.Length < 6 Then

                        InitialEntry = InitialEntry & "6"

                    End If

                End If

            Case Keys.D7

                ReturnToInitialEntryScreen()

                If TimerState = AppState.Initial Then

                    If InitialEntry.Length < 6 Then

                        InitialEntry = InitialEntry & "7"

                    End If

                End If

            Case Keys.D8

                ReturnToInitialEntryScreen()

                If TimerState = AppState.Initial Then

                    If InitialEntry.Length < 6 Then

                        InitialEntry = InitialEntry & "8"

                    End If

                End If

            Case Keys.D9

                ReturnToInitialEntryScreen()

                If TimerState = AppState.Initial Then

                    If InitialEntry.Length < 6 Then

                        InitialEntry = InitialEntry & "9"

                    End If

                End If

            Case Keys.NumPad0

                ReturnToInitialEntryScreen()

                If TimerState = AppState.Initial Then

                    If InitialEntry.Length < 6 Then

                        If Not InitialEntry = String.Empty Then

                            InitialEntry = InitialEntry & "0"

                        End If

                    End If

                End If

            Case Keys.NumPad1

                ReturnToInitialEntryScreen()

                If TimerState = AppState.Initial Then

                    If InitialEntry.Length < 6 Then

                        InitialEntry = InitialEntry & "1"

                    End If

                End If

            Case Keys.NumPad2

                ReturnToInitialEntryScreen()

                If TimerState = AppState.Initial Then

                    If InitialEntry.Length < 6 Then

                        InitialEntry = InitialEntry & "2"

                    End If

                End If

            Case Keys.NumPad3

                ReturnToInitialEntryScreen()

                If TimerState = AppState.Initial Then

                    If InitialEntry.Length < 6 Then

                        InitialEntry = InitialEntry & "3"

                    End If

                End If

            Case Keys.NumPad4

                ReturnToInitialEntryScreen()

                If TimerState = AppState.Initial Then

                    If InitialEntry.Length < 6 Then

                        InitialEntry = InitialEntry & "4"

                    End If

                End If

            Case Keys.NumPad5

                ReturnToInitialEntryScreen()

                If TimerState = AppState.Initial Then

                    If InitialEntry.Length < 6 Then

                        InitialEntry = InitialEntry & "5"

                    End If

                End If

            Case Keys.NumPad6

                ReturnToInitialEntryScreen()

                If TimerState = AppState.Initial Then

                    If InitialEntry.Length < 6 Then

                        InitialEntry = InitialEntry & "6"

                    End If

                End If

            Case Keys.NumPad7

                ReturnToInitialEntryScreen()

                If TimerState = AppState.Initial Then

                    If InitialEntry.Length < 6 Then

                        InitialEntry = InitialEntry & "7"

                    End If

                End If

            Case Keys.NumPad8

                ReturnToInitialEntryScreen()

                If TimerState = AppState.Initial Then

                    If InitialEntry.Length < 6 Then

                        InitialEntry = InitialEntry & "8"

                    End If

                End If

            Case Keys.NumPad9

                ReturnToInitialEntryScreen()

                If TimerState = AppState.Initial Then

                    If InitialEntry.Length < 6 Then

                        InitialEntry = InitialEntry & "9"

                    End If

                End If

            Case Keys.Enter

                Select Case TimerState

                    Case AppState.Initial

                        StartTimer()

                    Case AppState.Stopped

                        TimerState = AppState.Running

                        StartTime = Now

                    Case AppState.Completed

                        TimerState = AppState.Stopped

                    Case AppState.Paused

                        TogglePause()

                    Case AppState.Running

                        TogglePause()

                End Select

        End Select

    End Sub

    Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles MyBase.Closing

        CloseSounds()

    End Sub

    Protected Overrides Sub OnPaintBackground(ByVal e As PaintEventArgs)

        'Intentionally left blank. Do not remove.

    End Sub

    Private Sub UpdateDisplays()

        UpdateMainDisplay()

    End Sub

    Private Sub AllocateBuffer(e As PaintEventArgs)

        ' Allocate the buffer if it hasn't been allocated yet
        If Buffer Is Nothing Then

            Buffer = Context.Allocate(e.Graphics, ClientRectangle)

            With Buffer.Graphics

                .CompositingMode = Drawing2D.CompositingMode.SourceOver
                .TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias
                .SmoothingMode = Drawing2D.SmoothingMode.HighQuality
                .PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
                .CompositingQuality = Drawing2D.CompositingQuality.HighQuality
                .InterpolationMode = InterpolationMode.HighQualityBicubic
                .TextContrast = SmoothingMode.HighQuality

            End With

        End If

    End Sub

    Private Sub DisposeBuffer()

        ' Dispose of the existing buffer
        If Buffer IsNot Nothing Then

            Buffer.Dispose()

            Buffer = Nothing ' Set to Nothing to avoid using a disposed object

            ' The buffer will be reallocated in OnPaint

        End If

    End Sub

    Private Sub TogglePause()

        If TimerState = AppState.Running Then

            TimerState = AppState.Paused

            ' Store the elapsed time to resume from the same point
            ElapsedTime = DateTime.Now - StartTime

        ElseIf TimerState = AppState.Paused Then

            TimerState = AppState.Running

            ' Adjust the start time based on the paused duration
            StartTime = DateTime.Now - ElapsedTime

        End If

    End Sub

    Private Sub UpdateSound()

        Select Case TimerState

            Case AppState.Completed

                If Not IsPlaying("timesup") Then

                    LoopSound("timesup")

                End If

            Case AppState.Stopped

                If IsPlaying("timesup") Then

                    PauseSound("timesup")

                End If

        End Select

    End Sub

    Private Sub DrawDisplays()

        If Buffer IsNot Nothing Then

            Try

                With Buffer.Graphics

                    Select Case TimerState

                        Case AppState.Completed

                            .Clear(Color.Gray)

                            .DrawEllipse(CircleOfProgressBackgroundPen, CircleOfProgress)

                            .DrawString(MainDisplay.Text, MainDisplay.Font, Brushes.White, MainDisplay.Location, AlineCenterMiddle)

                            FillRoundedRectangle(Brushes.White, StopButton.Rect, StopButton.Radius, Buffer.Graphics)

                            .DrawString(StopButton.Text, StopButton.Font, Brushes.DimGray, StopButton.TextLocation, AlineCenterMiddle)

                            .DrawArc(CircleOfProgressPen, CircleOfProgress, startAngle, sweepAngle)

                            ' TODO: Status Display "Completed"

                            .DrawString(StatusDisplay.Text, StatusDisplay.Font, Brushes.White, StatusDisplay.Location, AlineCenterMiddle)

                        Case AppState.Running

                            .Clear(Color.Black)

                            .DrawEllipse(CircleOfProgressBackgroundPen, CircleOfProgress)

                            .DrawString(MainDisplay.Text, MainDisplay.Font, Brushes.White, MainDisplay.Location, AlineCenterMiddle)

                            FillRoundedRectangle(Brushes.White, PauseButton.Rect, PauseButton.Radius, Buffer.Graphics)

                            .DrawString(PauseButton.Text, PauseButton.Font, Brushes.Black, PauseButton.TextLocation, AlineCenterMiddle)

                            .DrawArc(CircleOfProgressPen, CircleOfProgress, startAngle, sweepAngle)

                            ' TODO: Status Display "Running"

                            .DrawString(StatusDisplay.Text, StatusDisplay.Font, Brushes.White, StatusDisplay.Location, AlineCenterMiddle)

                        Case AppState.Stopped

                            .Clear(Color.Black)

                            .DrawEllipse(CircleOfProgressBackgroundPen, CircleOfProgress)

                            .DrawString(MainDisplay.Text, MainDisplay.Font, Brushes.White, MainDisplay.Location, AlineCenterMiddle)

                            FillRoundedRectangle(Brushes.White, RestartButton.Rect, RestartButton.Radius, Buffer.Graphics)

                            .DrawString(RestartButton.Text, RestartButton.Font, Brushes.Black, RestartButton.TextLocation, AlineCenterMiddle)

                            ' TODO: Status Display "Stopped"

                            .DrawString(StatusDisplay.Text, StatusDisplay.Font, Brushes.White, StatusDisplay.Location, AlineCenterMiddle)

                        Case AppState.Initial

                            .Clear(Color.Black)

                            If Not InitialEntry = String.Empty Then

                                .DrawString(InitialDisplay.Text, InitialDisplay.Font, Brushes.SkyBlue, InitialDisplay.Location, AlineCenterMiddle)

                                FillRoundedRectangle(Brushes.White, StartButton.Rect, StartButton.Radius, Buffer.Graphics)

                                .DrawString(StartButton.Text, StartButton.Font, Brushes.Black, StartButton.TextLocation, AlineCenterMiddle)

                            Else

                                .DrawString(InitialDisplay.Text, InitialDisplay.Font, Brushes.LightGray, InitialDisplay.Location, AlineCenterMiddle)

                            End If

                        Case AppState.Paused

                            .Clear(Color.Black)

                            .DrawEllipse(CircleOfProgressBackgroundPen, CircleOfProgress)

                            .DrawString(MainDisplay.Text, MainDisplay.Font, Brushes.White, MainDisplay.Location, AlineCenterMiddle)

                            FillRoundedRectangle(Brushes.White, ResumeButton.Rect, ResumeButton.Radius, Buffer.Graphics)

                            .DrawString(ResumeButton.Text, ResumeButton.Font, Brushes.Black, ResumeButton.TextLocation, AlineCenterMiddle)

                            .DrawArc(CircleOfProgressPen, CircleOfProgress, startAngle, sweepAngle)

                            ' TODO: Status Display "Paused"

                            .DrawString(StatusDisplay.Text, StatusDisplay.Font, Brushes.White, StatusDisplay.Location, AlineCenterMiddle)


                    End Select

                End With

            Catch ex As Exception

                Debug.Print("Draw error: " & ex.Message)

            End Try

        Else

            Debug.Print("Buffer is not initialized.")

        End If

    End Sub

    Private Sub UpdateMainDisplay()

        Select Case TimerState

            Case AppState.Initial

                UpdateMainDisplayInitial()

            Case AppState.Running

                UpdateMainDisplayRunning()

            Case AppState.Completed

                'MainDisplay.Text = "0"

            Case AppState.Stopped

                UpdateMainDisplayStopped()

            Case AppState.Paused

                UpdateMainDisplayPaused()

        End Select

    End Sub

    Private Sub UpdateMainDisplayPaused()

        Dim RemainingTime As TimeSpan = Duration - ElapsedTime

        ' Do we have hours?
        If RemainingTime.Hours > 0 Then
            ' Yes, we have hours.

            ' Show hours, minutes and seconds.
            MainDisplay.Text = RemainingTime.ToString("h\:mm\:ss")

        Else
            ' No, we don't have hours.

            ' Do we have minutes?
            If RemainingTime.Minutes > 0 Then
                'Yes, we have minutes.

                ' Show minutes.
                MainDisplay.Text = RemainingTime.ToString("m\:ss")

            Else
                ' No, we don't have minutes.

                ' Do we have seconds?
                If RemainingTime.Seconds > 0 Then
                    ' Yes, we have seconds.

                    ' Show seconds.
                    MainDisplay.Text = RemainingTime.Seconds.ToString

                Else
                    ' No, we don't have seconds.

                    ' Show milliseconds.
                    MainDisplay.Text = RemainingTime.ToString("ff").TrimStart("0")

                End If

            End If

        End If

    End Sub

    Private Sub UpdateMainDisplayInitial()

        ' Has the user entered a time?
        If Not InitialEntry = String.Empty Then
            ' Yes, the user has entered a time.

            ' Ensure the input string is padded to at least 6 digits
            Dim PaddedInitialEntry As String = InitialEntry.PadLeft(6, "0"c)

            ' Extract hours, minutes, and seconds from the string
            Dim hours As Integer = Integer.Parse(PaddedInitialEntry.Substring(0, 2))
            Dim minutes As Integer = Integer.Parse(PaddedInitialEntry.Substring(2, 2))
            Dim seconds As Integer = Integer.Parse(PaddedInitialEntry.Substring(4, 2))

            InitialDisplay.Text = hours.ToString.PadLeft(2, "0"c) & "h " & minutes.ToString.PadLeft(2, "0"c) & "m " & seconds.ToString.PadLeft(2, "0"c) & "s"

        Else
            ' No, the user has not entered a time.

            ' Show zeros.
            InitialDisplay.Text = "00h " & "00m " & "00s"

        End If

    End Sub

    Private Sub UpdateMainDisplayStopped()

        ' Do we have hours?
        If Duration.Hours > 0 Then
            ' Yes, we have hours.

            ' Show hours, minutes and seconds.
            MainDisplay.Text = Duration.ToString("h\:mm\:ss")

        Else
            ' No, we don't have hours.

            ' Do we have minutes?
            If Duration.Minutes > 0 Then
                ' Yes, we have minutes.

                ' Show minutes and seconds.
                MainDisplay.Text = Duration.ToString("m\:ss")

            Else
                ' No, we don't have minutes.

                ' Do we have seconds?
                If Duration.Seconds > 0 Then
                    ' Yes, we have seconds.

                    ' Show seconds.
                    MainDisplay.Text = Duration.Seconds.ToString

                Else
                    ' No, we don't have seconds.

                    ' Show milliseconds.
                    MainDisplay.Text = Duration.ToString("ff").TrimStart("0")

                End If

            End If

        End If

    End Sub

    Private Sub UpdateMainDisplayRunning()

        ElapsedTime = DateTime.Now - StartTime

        Dim RemainingTime As TimeSpan = Duration - ElapsedTime

        ' Do we have hours?
        If RemainingTime.Hours > 0 Then
            ' Yes, we have hours.

            ' Show hours.
            MainDisplay.Text = RemainingTime.ToString("h\:mm\:ss")

        Else
            ' No, we don't have hours.

            ' Do we have minutes?
            If RemainingTime.Minutes > 0 Then
                'Yes, we have minutes.

                ' Show minutes.
                MainDisplay.Text = RemainingTime.ToString("m\:ss")

            Else
                ' No, we don't have minutes.

                ' Do we have seconds?
                If RemainingTime.Seconds > 0 Then
                    ' Yes, we have seconds.

                    ' Show seconds.
                    MainDisplay.Text = RemainingTime.Seconds.ToString

                Else
                    ' No, don't have seconds.

                    ' Show milliseconds.
                    MainDisplay.Text = RemainingTime.ToString("ff").TrimStart("0")

                End If

            End If

        End If

        ' Check if timer should complete
        If ElapsedTime.TotalSeconds >= Duration.TotalSeconds Then

            MainDisplay.Text = "0"

            TimerState = AppState.Completed

        End If

        RatioDegDuration = 360 / Duration.TotalSeconds

        sweepAngle = RatioDegDuration * ElapsedTime.TotalSeconds

    End Sub

    Private Sub InitializeForm()

        CenterToScreen()

        SetStyle(ControlStyles.UserPaint, True)

        SetStyle(ControlStyles.OptimizedDoubleBuffer, True)

        SetStyle(ControlStyles.AllPaintingInWmPaint, True)

        Text = "Timer⏳ - Code with Joe"

        Me.WindowState = FormWindowState.Maximized

    End Sub

    Private Sub InitializeBuffer()

        ' Set context to the context of this app.
        Context = BufferedGraphicsManager.Current

        Context.MaximumBuffer = Screen.PrimaryScreen.WorkingArea.Size

        ' Allocate the buffer initially using the current client rectangle
        Buffer = Context.Allocate(CreateGraphics(), ClientRectangle)

        With Buffer.Graphics

            .CompositingMode = Drawing2D.CompositingMode.SourceOver
            .TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias
            .SmoothingMode = Drawing2D.SmoothingMode.HighQuality
            .PixelOffsetMode = Drawing2D.PixelOffsetMode.None
            .CompositingQuality = Drawing2D.CompositingQuality.HighQuality

        End With

    End Sub

    Private Sub ReturnToInitialEntryScreen()

        If TimerState = AppState.Stopped Then

            TimerState = AppState.Initial

        End If

    End Sub

    Private Sub StartTimer()

        ' Did the user enter a duration?
        If Not InitialEntry = String.Empty Then
            ' Yes, the user did enter a duration.

            ' Ensure the input string is padded to at least 6 digits
            Dim PaddedInitialEntry = InitialEntry.PadLeft(6, "0"c)

            ' Extract hours, minutes, and seconds from the string
            Dim hours As Integer = Integer.Parse(PaddedInitialEntry.Substring(0, 2))
            Dim minutes As Integer = Integer.Parse(PaddedInitialEntry.Substring(2, 2))
            Dim seconds As Integer = Integer.Parse(PaddedInitialEntry.Substring(4, 2))

            If hours > 23 Then hours = 23
            If minutes > 59 Then minutes = 59
            If seconds > 59 Then seconds = 59

            ' Create and return the TimeSpan
            Duration = New TimeSpan(hours, minutes, seconds)

            InitialEntry = Duration.Hours.ToString & Duration.Minutes.ToString & Duration.Seconds.ToString

            TimerState = AppState.Running

            StartTime = Now

        End If

    End Sub

    Private Sub DeleteLastInitialEntryCharacter()

        If TimerState = AppState.Initial Then

            ' Are there character to delete?
            If InitialEntry.Length > 0 Then
                ' Yes, there are character to delete.

                ' Delete the last character in the initial entry string.
                InitialEntry = InitialEntry.Substring(0, InitialEntry.Length - 1)

            End If

        End If

    End Sub

    Private Sub ResizeResumeButton()

        Dim ButtonSize As Integer

        If ClientSize.Height / 14 > 32 Then

            ButtonSize = ClientSize.Height / 14

        Else

            ButtonSize = 32

        End If

        ResumeButton.Rect = New Rectangle(ClientSize.Width / 2 - ButtonSize / 2,
                                         ClientSize.Height / 2 + ButtonSize * 1.5,
                                         ButtonSize,
                                         ButtonSize)
        Dim FontSize As Single

        If ClientSize.Height / 40 > 10 Then

            FontSize = ClientSize.Height / 40

        Else

            FontSize = 10

        End If

        ResumeButton.Font = New Font("Segoe UI Symbol",
                                     FontSize,
                                     FontStyle.Regular)

        ResumeButton.TextLocation = New Point(ResumeButton.Rect.X + ResumeButton.Rect.Width / 2,
                                              ResumeButton.Rect.Y + ResumeButton.Rect.Height / 2)

        ResumeButton.Radius = ClientSize.Height / 30

    End Sub

    Private Sub ResizePauseButton()

        Dim ButtonSize As Integer

        If ClientSize.Height / 14 > 32 Then

            ButtonSize = ClientSize.Height / 14

        Else

            ButtonSize = 32

        End If

        PauseButton.Rect = New Rectangle(ClientSize.Width / 2 - ButtonSize / 2,
                                         ClientSize.Height / 2 + ButtonSize * 1.5,
                                         ButtonSize,
                                         ButtonSize)
        Dim FontSize As Single

        If ClientSize.Height / 45 > 10 Then

            FontSize = ClientSize.Height / 45

        Else

            FontSize = 10

        End If

        PauseButton.Font = New Font("Segoe UI Symbol",
                                    FontSize,
                                    FontStyle.Regular)

        PauseButton.TextLocation = New Point(PauseButton.Rect.X + PauseButton.Rect.Width / 2,
                                             PauseButton.Rect.Y + PauseButton.Rect.Height / 2)

        PauseButton.Radius = ClientSize.Height / 30

    End Sub

    Private Sub ResizeStartButton()

        Dim ButtonSize As Integer

        If ClientSize.Height / 14 > 32 Then

            ButtonSize = ClientSize.Height / 14

        Else

            ButtonSize = 32

        End If

        StartButton.Rect = New Rectangle(ClientSize.Width / 2 - ButtonSize / 2,
                                         ClientSize.Height / 2,
                                         ButtonSize,
                                         ButtonSize)

        Dim FontSize As Single

        If ClientSize.Height / 45 > 10 Then

            FontSize = ClientSize.Height / 45

        Else

            FontSize = 10

        End If

        StartButton.Font = New Font("Segoe UI Symbol",
                                    FontSize,
                                    FontStyle.Regular)

        StartButton.TextLocation = New Point(StartButton.Rect.X + StartButton.Rect.Width / 2,
                                             StartButton.Rect.Y + StartButton.Rect.Height / 2)

        Dim Radius As Integer

        If ClientSize.Height / 40 > 14 Then

            Radius = ClientSize.Height / 40

        Else

            Radius = 14

        End If

        StartButton.Radius = Radius

    End Sub

    Private Sub ResizeStopButton()

        Dim ButtonSize As Integer

        If ClientSize.Height / 14 > 32 Then

            ButtonSize = ClientSize.Height / 14

        Else

            ButtonSize = 32

        End If

        StopButton.Rect = New Rectangle(ClientSize.Width / 2 - ButtonSize / 2,
                                        ClientSize.Height / 2 + ButtonSize * 1.5,
                                        ButtonSize,
                                        ButtonSize)

        Dim FontSize As Single

        If ClientSize.Height / 45 > 10 Then

            FontSize = ClientSize.Height / 45

        Else

            FontSize = 10

        End If

        StopButton.Font = New Font("Segoe UI",
                                   FontSize,
                                   FontStyle.Regular)

        StopButton.TextLocation = New Point(StopButton.Rect.X + StopButton.Rect.Width / 2,
                                            StopButton.Rect.Y + StopButton.Rect.Height / 2)

        StopButton.Radius = ClientSize.Height / 30

    End Sub

    Private Sub ResizeRestartButton()

        Dim ButtonSize As Integer

        If ClientSize.Height / 14 > 32 Then

            ButtonSize = ClientSize.Height / 14

        Else

            ButtonSize = 32

        End If

        RestartButton.Rect = New Rectangle(ClientSize.Width / 2 - ButtonSize / 2,
                                           ClientSize.Height / 2 + ButtonSize * 1.5,
                                           ButtonSize,
                                           ButtonSize)

        Dim FontSize As Single

        If ClientSize.Height / 45 > 10 Then

            FontSize = ClientSize.Height / 45

        Else

            FontSize = 10

        End If

        RestartButton.Font = New Font("Segoe UI Symbol",
                                      FontSize,
                                      FontStyle.Regular)

        RestartButton.TextLocation = New Point(RestartButton.Rect.X + RestartButton.Rect.Width / 2,
                                               RestartButton.Rect.Y + RestartButton.Rect.Height / 2)

        RestartButton.Radius = ClientSize.Height / 30

    End Sub

    Private Sub ResizeInitialDisplay()

        Dim FontSize As Single

        If ClientSize.Height / 35 > 10 Then

            FontSize = ClientSize.Height / 35

        Else

            FontSize = 10

        End If

        Dim ButtonSize As Integer

        If ClientSize.Height / 14 > 32 Then

            ButtonSize = ClientSize.Height / 14

        Else

            ButtonSize = 32

        End If

        InitialDisplay.Font = New Font("Segoe UI",
                                       FontSize,
                                       FontStyle.Regular)

        InitialDisplay.Location.X = ClientSize.Width / 2

        InitialDisplay.Location.Y = ClientSize.Height / 2 - ButtonSize

    End Sub
    Private Sub ResizeMainDisplay()

        Dim FontSize As Single

        If ClientSize.Height / 13 > 35 Then

            FontSize = ClientSize.Height / 13

        Else

            FontSize = 35

        End If

        ' Set the font size for the main display based on the width of the client rectangle
        MainDisplay.Font = New Font("Segoe UI",
                                    FontSize,
                                    FontStyle.Regular)

        ' Center the main display in the client rectangle.
        MainDisplay.Location.X = ClientSize.Width / 2
        MainDisplay.Location.Y = ClientSize.Height / 2

    End Sub

    Private Sub ResizeStatusDisplay()

        Dim FontSize As Single

        If ClientSize.Height / 75 > 6 Then

            FontSize = ClientSize.Height / 75

        Else

            FontSize = 6

        End If

        Dim ButtonSize As Integer

        If ClientSize.Height / 14 > 32 Then

            ButtonSize = ClientSize.Height / 14

        Else

            ButtonSize = 32

        End If

        ' Set the font size for the  display based on the width of the client rectangle
        StatusDisplay.Font = New Font("Segoe UI",
                                    FontSize,
                                    FontStyle.Regular)

        ' Center the main display in the client rectangle.
        StatusDisplay.Location.X = ClientSize.Width / 2

        StatusDisplay.Location.Y = ClientSize.Height / 2 + ButtonSize * 3

    End Sub

    Private Sub ResizeCircleOfProgress()

        Dim PenSize As Single

        If ClientSize.Height / 30 > 15 Then

            PenSize = ClientSize.Height / 30

        Else

            PenSize = 15

        End If

        CircleOfProgressBackgroundPen = New Pen(Color.DimGray, PenSize)

        If ClientSize.Height / 40 > 10 Then

            PenSize = ClientSize.Height / 40

        Else

            PenSize = 10

        End If

        CircleOfProgressPen = New Pen(Color.LightSkyBlue, PenSize) With {
            .EndCap = LineCap.Round,
            .StartCap = LineCap.Round
        }

        Dim CircleSize As Single

        If ClientSize.Height / 1.5 > 300 Then

            CircleSize = ClientSize.Height / 1.5

        Else

            CircleSize = 300

        End If

        CircleOfProgress.Width = CircleSize
        CircleOfProgress.Height = CircleSize

        CircleOfProgress.X = ClientSize.Width / 2 - CircleOfProgress.Width / 2
        CircleOfProgress.Y = ClientSize.Height / 2 - CircleOfProgress.Height / 2

    End Sub

    Private Sub FillRoundedRectangle(brush As Brush, Rect As Rectangle, radius As Integer, g As Graphics)

        Dim Path As New GraphicsPath()

        'Add top line inside the top left and top right corners.
        Path.AddLine(Rect.Left + radius, Rect.Top, Rect.Right - radius, Rect.Top)

        'Add top right corner.
        Path.AddArc(Rect.Right - radius, Rect.Top, radius, radius, 270, 90)

        'Add right line inside the top right and bottom right corners.
        Path.AddLine(Rect.Right, Rect.Top + radius, Rect.Right, Rect.Bottom - radius)

        'Add bottom right corner.
        Path.AddArc(Rect.Right - radius, Rect.Bottom - radius, radius, radius, 0, 90)

        'Add bottom line inside the bottom left and the bottom right corners.
        Path.AddLine(Rect.Right - radius, Rect.Bottom, Rect.Left + radius, Rect.Bottom)

        'Add bottom left corner.
        Path.AddArc(Rect.Left, Rect.Bottom - radius, radius, radius, 90, 90)

        'Add left line inside the top left and bottom left corners.
        Path.AddLine(Rect.Left, Rect.Bottom - radius, Rect.Left, Rect.Top + radius)

        'Add top left corner.
        Path.AddArc(Rect.Left, Rect.Top, radius, radius, 180, 90)

        Path.CloseFigure()

        g.FillPath(brush, Path)

    End Sub

    Private Function AddSound(SoundName As String, FilePath As String) As Boolean

        'Do we have a name and does the file exist?
        If Not String.IsNullOrWhiteSpace(SoundName) AndAlso IO.File.Exists(FilePath) Then
            'Yes, we have a name and the file exists.

            Dim CommandOpen As String = $"open ""{FilePath}"" alias {SoundName}"

            Dim ReturnString As New StringBuilder(128)

            'Do we have sounds?
            If Sounds IsNot Nothing Then
                'Yes, we have sounds.

                'Is the sound in the array already?
                If Not Sounds.Contains(SoundName) Then
                    'No, the sound is not in the array.

                    'Did the sound file open?
                    If mciSendStringW(CommandOpen, ReturnString, 0, IntPtr.Zero) = 0 Then
                        'Yes, the sound file did open.

                        'Add the sound to the Sounds array.
                        Array.Resize(Sounds, Sounds.Length + 1)

                        Sounds(Sounds.Length - 1) = SoundName

                        Return True 'The sound was added.

                    End If

                End If

            Else
                'No, we do not have sounds.

                'Did the sound file open?
                If mciSendStringW(CommandOpen, ReturnString, 0, IntPtr.Zero) = 0 Then
                    'Yes, the sound file did open.

                    'Start the Sounds array with the sound.
                    ReDim Sounds(0)

                    Sounds(0) = SoundName

                    Return True 'The sound was added.

                End If

            End If

        End If

        Return False 'The sound was not added.

    End Function

    Private Function SetVolume(SoundName As String, Level As Integer) As Boolean

        'Do we have sounds?
        If Sounds IsNot Nothing Then
            'Yes, we have sounds.

            'Is the sound in the sounds array?
            If Sounds.Contains(SoundName) Then
                'Yes, the sound is the sounds array.

                'Is the level in the valid range?
                If Level >= 0 AndAlso Level <= 1000 Then
                    'Yes, the level is in range.

                    Dim CommandVolume As String = $"setaudio {SoundName} volume to {Level}"

                    Dim ReturnString As New StringBuilder(128)

                    'Was the volume set?
                    If mciSendStringW(CommandVolume, ReturnString, 0, IntPtr.Zero) = 0 Then

                        Return True 'The volume was set.

                    End If

                End If

            End If

        End If

        Return False 'The volume was not set.

    End Function

    Private Function LoopSound(SoundName As String) As Boolean

        ' Do we have sounds?
        If Sounds IsNot Nothing Then
            ' Yes, we have sounds.

            ' Is the sound in the array?
            If Not Sounds.Contains(SoundName) Then
                ' No, the sound is not in the array.

                Return False ' The sound is not playing.

            End If

            Dim CommandSeekToStart As String = $"seek {SoundName} to start"

            Dim ReturnString As New StringBuilder(128)

            mciSendStringW(CommandSeekToStart, ReturnString, 0, IntPtr.Zero)

            Dim CommandPlayRepete As String = $"play {SoundName} repeat"

            If mciSendStringW(CommandPlayRepete, ReturnString, 0, Me.Handle) <> 0 Then

                Return False ' The sound is not playing.

            End If

        End If

        Return True ' The sound is playing.

    End Function

    Private Function PlaySound(SoundName As String) As Boolean

        'Do we have sounds?
        If Sounds IsNot Nothing Then
            'Yes, we have sounds.

            'Is the sound in the array?
            If Sounds.Contains(SoundName) Then
                'Yes, the sound is in the array.

                Dim CommandSeekToStart As String = $"seek {SoundName} to start"

                Dim ReturnString As New StringBuilder(128)

                mciSendStringW(CommandSeekToStart, ReturnString, 0, IntPtr.Zero)

                Dim CommandPlay As String = $"play {SoundName} notify"

                If mciSendStringW(CommandPlay, ReturnString, 0, Me.Handle) = 0 Then

                    Return True 'The sound is playing.

                End If

            End If

        End If

        Return False 'The sound is not playing.

    End Function

    Private Function PauseSound(SoundName As String) As Boolean

        'Do we have sounds?
        If Sounds IsNot Nothing Then
            'Yes, we have sounds.

            'Is the sound in the array?
            If Sounds.Contains(SoundName) Then
                'Yes, the sound is in the array.

                Dim CommandPause As String = $"pause {SoundName} notify"

                Dim ReturnString As New StringBuilder(128)

                If mciSendStringW(CommandPause, ReturnString, 0, Me.Handle) = 0 Then

                    Return True 'The sound is paused.

                End If

            End If

        End If

        Return False 'The sound is not paused.

    End Function

    Private Function IsPlaying(SoundName As String) As Boolean

        Return GetStatus(SoundName, "mode") = "playing"

    End Function

    Private Sub AddOverlapping(SoundName As String, FilePath As String)

        For Each Suffix As String In {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L"}

            AddSound(SoundName & Suffix, FilePath)

        Next

    End Sub

    Private Sub PlayOverlapping(SoundName As String)

        For Each Suffix As String In {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L"}

            If Not IsPlaying(SoundName & Suffix) Then

                PlaySound(SoundName & Suffix)

                Exit Sub

            End If

        Next

    End Sub

    Private Sub SetVolumeOverlapping(SoundName As String, Level As Integer)

        For Each Suffix As String In {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L"}

            SetVolume(SoundName & Suffix, Level)

        Next

    End Sub

    Private Function GetStatus(SoundName As String, StatusType As String) As String

        If Sounds IsNot Nothing Then

            If Sounds.Contains(SoundName) Then

                Dim Command As String = $"status {SoundName} {StatusType}"

                Dim ReturnString As New StringBuilder(128)

                ' Send the command to get the status
                If mciSendStringW(Command, ReturnString, ReturnString.Capacity, IntPtr.Zero) = 0 Then

                    Return ReturnString.ToString() ' Return the status if successful

                End If

            End If

        End If

        Return String.Empty ' Return an empty string if there's an error

    End Function

    Private Sub CloseSounds()

        If Sounds IsNot Nothing Then

            For Each Sound In Sounds

                Dim CommandClose As String = $"close {Sound}"

                Dim ReturnString As New StringBuilder(128)

                mciSendStringW(CommandClose, ReturnString, 0, IntPtr.Zero)

            Next

        End If

    End Sub

    Private Sub CreateSoundFileFromResource()

        Dim FilePath As String = Path.Combine(Application.StartupPath, "timesup.mp3")

        If Not IO.File.Exists(FilePath) Then

            IO.File.WriteAllBytes(FilePath, My.Resources.Resource1.TimesUp)

        End If

    End Sub

End Class

' Monica is our an AI assistant.
' https://monica.im/