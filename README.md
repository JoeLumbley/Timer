# Timer⏳

This application is designed to provide an intuitive and user-friendly countdown timer experience.



![005](https://github.com/user-attachments/assets/7f4610e0-fc43-4fef-9d50-8d55ddf7b24e)



## Features

- **Countdown Timer**: Easily set and track countdowns for various activities.
- **Pause/Resume Functionality**: Take control of your timing with the ability to pause and resume as needed.
- **Customizable Duration**: Set your desired timer duration with ease.
- **Sound Effects**: Enjoy auditory alerts with integrated sound effects when the timer completes.
- **Responsive UI**: The graphical interface adapts seamlessly to different window sizes, ensuring a consistent user experience across devices.
  
---

## Getting Started

Clone this repository and run the application to start using the timer. Whether you're timing a workout, cooking, or managing tasks, this timer is here to help you stay on track!



[Watch](https://youtube.com/shorts/n8bCEIdI44U?si=3LD1MtsabNRh-v7E) how to Clone a GitHub Repository - YouTube

---


## Code Walkthrough

### Imports and Class Declaration
```vb
Imports System.ComponentModel
Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text

Public Class Form1
```
- **Imports**: These lines bring in various namespaces that allow us to use specific classes and functions. For example:
  - `System.ComponentModel` is used for components and controls.
  - `System.Drawing.Drawing2D` allows for advanced 2D graphics.
  - `System.IO` is used for input and output operations (like file handling).
  - `System.Runtime.InteropServices` is used for calling functions from unmanaged code (like Windows API).
  - `System.Text` provides classes for handling text and string manipulation.

- **Public Class Form1**: This declares a new class named `Form1`, which is the main form of the application. All the code related to the timer functionality will be inside this class.

### Enum Declaration
```vb
Public Enum AppState
    Initial
    Running
    Paused
    Stopped
    Completed
End Enum
```
- **Enum**: This defines an enumeration called `AppState`. Enums are a way to define a set of named constants. Here, it represents the different states of the timer:
  - `Initial`: The timer has not started yet.
  - `Running`: The timer is currently counting down.
  - `Paused`: The timer is temporarily halted.
  - `Stopped`: The timer has been stopped and can be reset.
  - `Completed`: The timer has finished counting down.

### Variable Declarations
```vb
Private TimerState As AppState = AppState.Initial
Private StartTime As DateTime = Now
Private Duration As New TimeSpan(0, 0, 10)
Private InitialEntry As String = String.Empty
```
- **TimerState**: A variable to keep track of the current state of the timer, initialized to `Initial`.
- **StartTime**: A variable to store the time when the timer starts. `Now` gets the current date and time.
- **Duration**: A `TimeSpan` object initialized to 10 seconds. This sets the default countdown duration.
- **InitialEntry**: A string that will hold the user’s input for the countdown time, initialized to an empty string.

### PInvoke Declaration
```vb
<DllImport("winmm.dll", EntryPoint:="mciSendStringW")>
Private Shared Function mciSendStringW(<MarshalAs(UnmanagedType.LPTStr)> ByVal lpszCommand As String, <MarshalAs(UnmanagedType.LPWStr)> ByVal lpszReturnString As StringBuilder, ByVal cchReturn As UInteger, ByVal hwndCallback As IntPtr) As Integer
End Function
```
- **DllImport**: This attribute is used for calling a function from an unmanaged DLL (in this case, `winmm.dll`, which is a Windows multimedia library).
- **mciSendStringW**: This function sends commands to the multimedia control interface (MCI) to control audio playback.
- **Parameters**:
  - `lpszCommand`: The command to execute (like play, stop).
  - `lpszReturnString`: A buffer for any return string.
  - `cchReturn`: The size of the return string buffer.
  - `hwndCallback`: A handle to a window for notifications.

### Structure Declarations
```vb
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
```
- **Structures**: These are custom data types that can hold multiple related variables.
  - `DisplayObject`: Holds properties for displaying text on the screen, including its location, the text itself, and the font.
  - `ButtonStruct`: Holds properties for buttons, including their rectangle area, radius for rounded corners, text, text location, and font.

### Main Display Variables
```vb
Private MainDisplay As DisplayObject
Private TopDisplay As DisplayObject
Private BottomDisplay As DisplayObject
Private InitialDisplay As DisplayObject
```
- **Display Variables**: These variables will hold the display objects for the main timer display and other UI components.

### Progress Circle Variables
```vb
Private CircleOfProgress As Rectangle
Private CircleOfProgressPen As Pen
Private CircleOfProgressBackgroundPen As Pen
Private RatioDegDuration As Single
Private Const startAngle As Single = -90.0F
Private sweepAngle As Single
```
- **Progress Circle**: These variables will manage the circular progress indicator that visually represents the countdown.
  - `CircleOfProgress`: A rectangle representing the bounds of the circle.
  - `CircleOfProgressPen` and `CircleOfProgressBackgroundPen`: Pens to draw the circle and its background.
  - `RatioDegDuration`: A ratio to calculate the degree of the sweep based on the total duration.
  - `startAngle`: The starting angle for the drawing of the circle (set to -90 degrees for a top-start position).
  - `sweepAngle`: The angle that will be drawn based on elapsed time.

### Button Variables
```vb
Private StopButton As ButtonStruct
Private StartButton As ButtonStruct
Private RestartButton As ButtonStruct
Private PauseButton As ButtonStruct
Private ResumeButton As ButtonStruct
```
- **Button Variables**: These hold the button structures for different timer actions like stop, start, restart, pause, and resume.

### Load Event Handler
```vb
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
    Timer1.Interval = 15
    Timer1.Enabled = True
    Debug.Print($"Program running... {Now.ToShortTimeString}")
End Sub
```
- **Form1_Load**: This is an event handler that runs when the form is loaded.
- **InitializeForm()**: Sets up the form’s initial properties (like size, title).
- **InitializeBuffer()**: Prepares a buffer for double buffering to reduce flicker during drawing.
- **CreateSoundFileFromResource()**: Creates a sound file from embedded resources if it doesn’t exist.
- **AddSound**: Adds a sound file for the timer completion alert.
- **SetVolume**: Sets the volume for the sound.
- **Button Text**: Sets the text for the buttons (Stop, Start, Restart, Pause, Resume).
- **Timer1.Interval**: Sets the timer interval (how often the timer ticks).
- **Timer1.Enabled**: Starts the timer.
- **Debug.Print**: Outputs a debug message to the console.

### Resize Event Handler
```vb
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
        DisposeBuffer()
    End If
End Sub
```
- **Form1_Resize**: This event handler runs when the form is resized.
- **WindowState Check**: Ensures the resizing logic doesn’t run when the window is minimized.
- **Resize Methods**: Calls various methods to resize UI elements according to the new window size.
- **DisposeBuffer**: Disposes of the existing drawing buffer to free up resources.

### Timer Tick Event Handler
```vb
Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
    UpdateDisplays()
    If Not WindowState = FormWindowState.Minimized Then
        Refresh() '' Calls OnPaint Sub
    End If
    UpdateSound()
End Sub
```
- **Timer1_Tick**: This handler runs every time the timer ticks (based on the interval).
- **UpdateDisplays()**: Updates the display elements (like the countdown).
- **WindowState Check**: Refreshes the display only if the window is not minimized.
- **UpdateSound()**: Checks if the sound needs to be played based on the timer state.

### OnPaint Override
```vb
Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
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
    DrawDisplays()
    Buffer.Render(e.Graphics)
End Sub
```
- **OnPaint**: This method is called to paint the form.
- **Buffer Allocation**: Allocates a graphics buffer if it hasn’t been allocated yet.
- **Graphics Settings**: Sets various rendering settings for high-quality graphics.
- **DrawDisplays()**: Calls a method to draw the current display elements.
- **Buffer.Render**: Renders the buffer onto the form.

### Mouse Down Event Handler
```vb
Private Sub Form1_MouseDown(sender As Object, e As MouseEventArgs) Handles Me.MouseDown
    Select Case TimerState
        Case AppState.Completed
            If StopButton.Rect.Contains(e.Location) Then
                TimerState = AppState.Stopped
            End If
        Case AppState.Initial
            If StartButton.Rect.Contains(e.Location) Then
                StartTimer()
            End If
        Case AppState.Stopped
            If RestartButton.Rect.Contains(e.Location) Then
                TimerState = AppState.Running
                StartTime = Now
            End If
        Case AppState.Running
            If PauseButton.Rect.Contains(e.Location) Then
                TogglePause()
            End If
        Case AppState.Paused
            TogglePause()
    End Select
End Sub
```
- **Form1_MouseDown**: This event handler runs when the mouse is clicked on the form.
- **Select Case**: Checks the current `TimerState` to determine what action to take based on the button clicked:
  - **Completed**: If the stop button is clicked, change the state to `Stopped`.
  - **Initial**: If the start button is clicked, call `StartTimer()`.
  - **Stopped**: If the restart button is clicked, set the state to `Running` and reset the start time.
  - **Running**: If the pause button is clicked, call `TogglePause()`.
  - **Paused**: If clicked, it resumes the timer.

### Key Down Event Handler
```vb
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
        ' Additional cases for number keys (D0-D9 and NumPad0-NumPad9)
        ' to add numbers to InitialEntry for timer duration
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
```
- **Form1_KeyDown**: This event handler runs when a key is pressed.
- **Select Case e.KeyValue**: Checks which key was pressed and performs actions based on the key:
  - **Back, Delete, X**: Return to the initial entry screen and delete the last character from input.
  - **Escape**: Resets the timer state based on its current state.
  - **Pause, P**: Toggles the pause state of the timer.
  - **Number Keys**: Allows users to input numbers for the timer duration.
  - **Enter**: Starts the timer or resumes it based on the current state.

### Closing Event Handler
```vb
Private Sub Form1_Closing(sender As Object, e As CancelEventArgs) Handles MyBase.Closing
    CloseSounds()
End Sub
```
- **Form1_Closing**: This event handler runs when the form is closing.
- **CloseSounds()**: Calls a method to close any open sound files, ensuring there are no lingering sounds.

### Update Displays Method
```vb
Private Sub UpdateDisplays()
    UpdateMainDisplay()
End Sub
```
- **UpdateDisplays**: This method updates the displays on the form by calling the `UpdateMainDisplay()` method.

### Dispose Buffer Method
```vb
Private Sub DisposeBuffer()
    If Buffer IsNot Nothing Then
        Buffer.Dispose()
        Buffer = Nothing
    End If
End Sub
```
- **DisposeBuffer**: This method disposes of the graphics buffer to free up resources if it exists.

### Draw Displays Method
```vb
Private Sub DrawDisplays()
    If Buffer IsNot Nothing Then
        Try
            With Buffer.Graphics
                Select Case TimerState
                    Case AppState.Completed
                        .Clear(Color.LightSkyBlue)
                        .DrawEllipse(CircleOfProgressBackgroundPen, CircleOfProgress)
                        .DrawString(MainDisplay.Text, MainDisplay.Font, Brushes.MidnightBlue, MainDisplay.Location, AlineCenterMiddle)
                        FillRoundedRectangle(Brushes.White, StopButton.Rect, StopButton.Radius, Buffer.Graphics)
                        .DrawString(StopButton.Text, StopButton.Font, Brushes.DimGray, StopButton.TextLocation, AlineCenterMiddle)
                    ' Additional cases for other timer states...
                End Select
            End With
        Catch ex As Exception
            Debug.Print("Draw error: " & ex.Message)
        End Try
    Else
        Debug.Print("Buffer is not initialized.")
    End If
End Sub
```
- **DrawDisplays**: This method draws the current state of the timer on the buffer.
- **Buffer Check**: Ensures the buffer exists before attempting to draw.
- **Graphics Drawing**: Uses the `With` statement to simplify drawing operations based on the current timer state.
- **Error Handling**: Catches any exceptions during drawing and prints an error message.

















## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.txt) file for more details.


