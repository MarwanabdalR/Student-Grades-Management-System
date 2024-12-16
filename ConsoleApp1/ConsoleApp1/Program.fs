﻿open System
open System.Windows.Forms
open System.Drawing
open System.IO
open Newtonsoft.Json

// Define student record
type Student = {
    ID: int
    Name: string
    Grades: list<int>
}

// Application state
let mutable isAdmin = false // Default role as Viewer
let dataFilePath = "students.json" // File to save and load data

let mutable students: list<Student> = []

// Helper functions
let calculateAverage (grades: int list) =
    // if grades.IsEmpty then 0.0 else grades |> List.averageBy float
    match grades with
    | [] -> 0.0 // If the list is empty, return 0.0
    | _ -> grades |> List.averageBy float // Otherwise, calculate the average

let getClassStatistics students =
    let allGrades = students |> List.collect (fun s -> s.Grades)
    let average = if allGrades.IsEmpty then 0.0 else allGrades |> List.averageBy float
    let highest = if allGrades.IsEmpty then 0 else List.max allGrades
    let lowest = if allGrades.IsEmpty then 0 else List.min allGrades
    let passRate =
        if allGrades.IsEmpty then 0.0
        else
            let passed = allGrades |> List.filter (fun g -> g >= 50)
            let passCount = float (List.length passed)
            let totalCount = float (List.length allGrades)
            (passCount / totalCount) * 100.0
    (average, highest, lowest, passRate)

// Save and Load Data
let saveData () =
    try
        File.WriteAllText(dataFilePath, JsonConvert.SerializeObject(students))
    with
    | ex -> MessageBox.Show($"Error saving data: {ex.Message}") |> ignore

let loadData () =
    try
        if File.Exists(dataFilePath) then
            let json = File.ReadAllText(dataFilePath)
            students <- JsonConvert.DeserializeObject<List<Student>>(json) |> List.ofSeq
    with
    | ex -> MessageBox.Show($"Error loading data: {ex.Message}") |> ignore

// GUI Elements
let form = new Form(Text = "Student Grades Management System", Width = 900, Height = 700, BackColor = Color.DarkSlateGray, Font = new Font("Arial", 10.0F, FontStyle.Bold))

let mainPanel = new Panel(Dock = DockStyle.Fill, BackColor = Color.FromArgb(32, 178, 170), Padding = Padding(20))
form.Controls.Add(mainPanel)

// Admin Panel
// Admin Panel Configuration
let adminPanel = 
    new Panel(
        Dock = DockStyle.Fill, 
        BackColor = Color.FromArgb(39, 174, 96), 
        Padding = Padding(20)
    )

// Title Label
let adminTitle = 
    new Label(
        Text = "Admin Panel", 
        Font = new Font("Arial", 24.0F, FontStyle.Bold), 
        ForeColor = Color.White, 
        TextAlign = ContentAlignment.MiddleCenter,
        AutoSize = true
    )
adminTitle.Location <- Point((adminPanel.Width - adminTitle.Width) / 2, 20)

// Labels and TextBoxes
let createLabel text yOffset =
    let lbl = 
        new Label(
            Text = text, 
            ForeColor = Color.White, 
            Font = new Font("Arial", 12.0F, FontStyle.Bold), 
            AutoSize = true
        )
    lbl.Location <- Point(50, yOffset)
    lbl

let createTextBox placeholder yOffset =
    let txt = 
        new TextBox(
            PlaceholderText = placeholder, 
            Width = 300, 
            Height = 30, 
            BackColor = Color.WhiteSmoke, 
            ForeColor = Color.Black
        )
    txt.Location <- Point((adminPanel.Width - txt.Width) / 2, yOffset)
    txt.Anchor <- AnchorStyles.None
    txt

let lblID = createLabel "Student ID" 80
let txtID = createTextBox "Enter Student ID" 110

let lblName = createLabel "Student Name" 160
let txtName = createTextBox "Enter Student Name" 190

let lblGrades = createLabel "Grades (comma-separated)" 240
let txtGrades = createTextBox "Enter Grades" 270

// Buttons
let createButton text yOffset color =
    let btn = 
        new Button(
            Text = text, 
            Width = 250, 
            Height = 40, 
            BackColor = color, 
            ForeColor = Color.White, 
            FlatStyle = FlatStyle.Flat
        )
    btn.Location <- Point((adminPanel.Width - btn.Width) / 2, yOffset)
    btn.Anchor <- AnchorStyles.None
    btn.FlatAppearance.BorderSize <- 0
    btn

let btnAdd = createButton "Add Student" 330 Color.LimeGreen
let btnEdit = createButton "Edit Student" 380 Color.DodgerBlue
let btnRemove = createButton "Remove Student" 430 Color.Tomato
let btnShowAllStudents = createButton "Show All Students" 480 Color.Goldenrod
let btnReturnToLogin = createButton "Return to Login" 530 Color.LightSlateGray

// Resize Event to Center Elements Dynamically
adminPanel.Resize.Add(fun _ -> 
    adminTitle.Location <- Point((adminPanel.Width - adminTitle.Width) / 2, 20)
    txtID.Location <- Point((adminPanel.Width - txtID.Width) / 2, 110)
    txtName.Location <- Point((adminPanel.Width - txtName.Width) / 2, 190)
    txtGrades.Location <- Point((adminPanel.Width - txtGrades.Width) / 2, 270)
    btnAdd.Location <- Point((adminPanel.Width - btnAdd.Width) / 2, 330)
    btnEdit.Location <- Point((adminPanel.Width - btnEdit.Width) / 2, 380)
    btnRemove.Location <- Point((adminPanel.Width - btnRemove.Width) / 2, 430)
    btnShowAllStudents.Location <- Point((adminPanel.Width - btnShowAllStudents.Width) / 2, 480)
    btnReturnToLogin.Location <- Point((adminPanel.Width - btnReturnToLogin.Width) / 2, 530)
)

// Add Controls to Admin Panel
adminPanel.Controls.AddRange([| 
    adminTitle
    lblID; txtID
    lblName; txtName
    lblGrades; txtGrades
    btnAdd; btnEdit; btnRemove; btnShowAllStudents; btnReturnToLogin
|])

// Viewer Panel
// Viewer Panel Configuration
let viewerPanel = 
    new Panel(
        Dock = DockStyle.Fill, 
        BackColor = Color.FromArgb(52, 73, 94), 
        Padding = Padding(20)
    )

// Title Label
let viewerTitle = 
    new Label(
        Text = "Viewer Panel", 
        Font = new Font("Arial", 24.0F, FontStyle.Bold), 
        ForeColor = Color.White, 
        TextAlign = ContentAlignment.MiddleCenter, 
        AutoSize = true
    )
viewerTitle.Location <- Point((viewerPanel.Width - viewerTitle.Width) / 2, 20)

// List Box for Viewing Students
let lstView = 
    new ListBox(
        Width = 500, 
        Height = 300, 
        BackColor = Color.WhiteSmoke, 
        ForeColor = Color.Black, 
        Font = new Font("Arial", 10.0F, FontStyle.Regular)
    )
lstView.Location <- Point((viewerPanel.Width - lstView.Width) / 2, 80)
lstView.Anchor <- AnchorStyles.None
lstView.BorderStyle <- BorderStyle.FixedSingle

// Statistics Label
let statisticsLabel = 
    new Label(
        Text = "Statistics: (Will update dynamically)",
        AutoSize = true,
        Font = new Font("Arial", 12.0F, FontStyle.Italic), 
        ForeColor = Color.White
    )
statisticsLabel.Location <- Point((viewerPanel.Width - statisticsLabel.Width) / 2, 400)

// Button to Return to Login
let btnBackToLoginViewer = 
    new Button(
        Text = "Return to Login", 
        Width = 200, 
        Height = 40, 
        BackColor = Color.LightSlateGray, 
        ForeColor = Color.White, 
        FlatStyle = FlatStyle.Flat
    )
btnBackToLoginViewer.Location <- Point((viewerPanel.Width - btnBackToLoginViewer.Width) / 2, 460)
btnBackToLoginViewer.FlatAppearance.BorderSize <- 0
btnBackToLoginViewer.Anchor <- AnchorStyles.None

// Resize Event for Centering Elements Dynamically
viewerPanel.Resize.Add(fun _ -> 
    viewerTitle.Location <- Point((viewerPanel.Width - viewerTitle.Width) / 2, 20)
    lstView.Location <- Point((viewerPanel.Width - lstView.Width) / 2, 80)
    statisticsLabel.Location <- Point((viewerPanel.Width - statisticsLabel.Width) / 2, 400)
    btnBackToLoginViewer.Location <- Point((viewerPanel.Width - btnBackToLoginViewer.Width) / 2, 460)
)

// Add Controls to Viewer Panel
viewerPanel.Controls.AddRange([| 
    viewerTitle; lstView; statisticsLabel; btnBackToLoginViewer 
|])

// Login Panel
let loginPanel = new Panel(Dock = DockStyle.Fill, BackColor = Color.FromArgb(41, 128, 185), Padding = Padding(20))
let loginTitle = new Label(Text = "Login", Dock = DockStyle.Top, Font = new Font("Arial", 20.0F, FontStyle.Bold), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter)
// Create and configure Username TextBox
let txtUsername = 
    new TextBox(
        PlaceholderText = "Username",
        Width = 300, // Set desired width
        Height = 30, // Set desired height
        BackColor = Color.WhiteSmoke,
        ForeColor = Color.Black
    )
txtUsername.Location <- Point((loginPanel.Width - txtUsername.Width) / 2, 150) // Center horizontally
txtUsername.Anchor <- AnchorStyles.None // Prevent automatic resizing

// Create and configure Password TextBox
let txtPassword = 
    new TextBox(
        PlaceholderText = "Password",
        PasswordChar = '*',
        Width = 300, // Set desired width
        Height = 60, // Set desired height
        BackColor = Color.WhiteSmoke,
        ForeColor = Color.Black
    )
txtPassword.Location <- Point((loginPanel.Width - txtPassword.Width) / 2, 200) // Center horizontally with vertical offset
txtPassword.Anchor <- AnchorStyles.None // Prevent automatic resizing

// Handle resizing to maintain centering
loginPanel.Resize.Add(fun _ -> 
    txtUsername.Location <- Point((loginPanel.Width - txtUsername.Width) / 2, 150)
    txtPassword.Location <- Point((loginPanel.Width - txtPassword.Width) / 2, 200)
)

let btnLogin = 
    new Button(
        Text = "Login",
        Height = 40,
        Padding = Padding(15),
        AutoSize = true,
        BackColor = Color.MediumSeaGreen,
        ForeColor = Color.White
    )

btnLogin.Location <- Point((loginPanel.Width - btnLogin.Width) / 2, 250)
btnLogin.Anchor <- AnchorStyles.None

loginPanel.Resize.Add(fun _ -> 
    btnLogin.Location <- Point((loginPanel.Width - btnLogin.Width) / 2, 250)
)

loginPanel.Controls.AddRange([| loginTitle; txtUsername; txtPassword; btnLogin |])

mainPanel.Controls.Add(loginPanel)

// Add interactions
let updateViewerPanel() =
    lstView.Items.Clear()
    students |> List.iter (fun s -> 
        let avg = calculateAverage s.Grades
        lstView.Items.Add(sprintf "ID: %d, Name: %s, Avg: %.2f" s.ID s.Name avg) |> ignore
    )
    let avg, high, low, passRate = getClassStatistics students
    statisticsLabel.Text <- sprintf "Class Avg: %.2f, High: %d, Low: %d, Pass Rate: %.2f%%" avg high low passRate

// Function to clear input fields
let clearInputFields () =
    txtID.Clear()
    txtName.Clear()
    txtGrades.Clear()

// Add interactions
btnAdd.Click.Add(fun _ -> 
    if isAdmin then
        try
            let id = int txtID.Text
            let name = txtName.Text
            let grades = txtGrades.Text.Split(',') |> Array.map int |> Array.toList
            students <- { ID = id; Name = name; Grades = grades } :: students
            MessageBox.Show("Student added successfully!") |> ignore
            updateViewerPanel()
            saveData()
            clearInputFields() // Clear input fields after adding
        with
        | _ -> MessageBox.Show("Invalid input!") |> ignore
    else MessageBox.Show("Only Admins can add students!") |> ignore
)

btnEdit.Click.Add(fun _ -> 
    if isAdmin then
        try
            let id = int txtID.Text
            match students |> List.tryFind (fun s -> s.ID = id) with
            | Some student ->
                let name = txtName.Text
                let grades = txtGrades.Text.Split(',') |> Array.map int |> Array.toList
                students <- students |> List.map (fun s -> if s.ID = id then { s with Name = name; Grades = grades } else s)
                MessageBox.Show("Student edited successfully!") |> ignore
                updateViewerPanel()
                saveData()
                clearInputFields() // Clear input fields after editing
            | None -> MessageBox.Show("Student not found!") |> ignore
        with
        | _ -> MessageBox.Show("Invalid input!") |> ignore
    else MessageBox.Show("Only Admins can edit students!") |> ignore
)

btnRemove.Click.Add(fun _ -> 
    if isAdmin then
        try
            let id = int txtID.Text
            students <- students |> List.filter (fun s -> s.ID <> id)
            MessageBox.Show("Student removed successfully!") |> ignore
            updateViewerPanel()
            saveData()
            clearInputFields() // Clear input fields after removing
        with
        | _ -> MessageBox.Show("Invalid input!") |> ignore
    else MessageBox.Show("Only Admins can remove students!") |> ignore
)

btnShowAllStudents.Click.Add(fun _ -> 
    if isAdmin then
        updateViewerPanel()
        mainPanel.Controls.Clear()
        mainPanel.Controls.Add(viewerPanel)
    else MessageBox.Show("Only Admins can view students!") |> ignore
)

btnReturnToLogin.Click.Add(fun _ -> 
    mainPanel.Controls.Clear()
    mainPanel.Controls.Add(loginPanel)
)

btnBackToLoginViewer.Click.Add(fun _ -> 
    mainPanel.Controls.Clear()
    mainPanel.Controls.Add(loginPanel)
)

btnLogin.Click.Add(fun _ -> 
    let username = txtUsername.Text
    let password = txtPassword.Text
    if username = "admin" && password = "admin" then
        isAdmin <- true
        mainPanel.Controls.Clear()
        mainPanel.Controls.Add(adminPanel)
    else
        MessageBox.Show("Invalid username or password!") |> ignore
)

Application.Run(form)
