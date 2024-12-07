open System
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
let mutable students: list<Student> = []
let mutable isAdmin = false // Default role as Viewer
let dataFilePath = "students.json" // File to save and load data

// Helper functions
let calculateAverage (grades: int list) =
    if grades.IsEmpty then 0.0 else grades |> List.averageBy float

let getClassStatistics students =
    let allGrades = students |> List.collect (fun s -> s.Grades)
    
    // Calculate average grade
    let average = 
        if allGrades.IsEmpty then 0.0 
        else allGrades |> List.averageBy float
    
    // Find highest and lowest grades
    let highest = 
        if allGrades.IsEmpty then 0 
        else List.max allGrades
    
    let lowest = 
        if allGrades.IsEmpty then 0 
        else List.min allGrades
    
    // Calculate pass rate
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
let form = new Form(Text = "Student Grades Management", Width = 800, Height = 600, BackColor = Color.Beige)

// Main Panel
let mainPanel = new Panel(Dock = DockStyle.Fill, BackColor = Color.LightGray)
form.Controls.Add(mainPanel)

// Admin Panel (for CRUD operations)
let adminPanel = new Panel(Dock = DockStyle.Top, Height = 300, BackColor = Color.FromArgb(200, 230, 250))
let lblID = new Label(Text = "Student ID", Dock = DockStyle.Top, ForeColor = Color.Blue)
let txtID = new TextBox(PlaceholderText = "Student ID", Dock = DockStyle.Top)
let lblName = new Label(Text = "Student Name", Dock = DockStyle.Top, ForeColor = Color.Blue)
let txtName = new TextBox(PlaceholderText = "Student Name", Dock = DockStyle.Top)
let lblGrades = new Label(Text = "Grades (comma-separated)", Dock = DockStyle.Top, ForeColor = Color.Blue)
let txtGrades = new TextBox(PlaceholderText = "Grades (comma-separated)", Dock = DockStyle.Top)
let btnAdd = new Button(Text = "Add Student", Dock = DockStyle.Top, BackColor = Color.LightGreen)
let btnEdit = new Button(Text = "Edit Student", Dock = DockStyle.Top, BackColor = Color.LightBlue)
let btnRemove = new Button(Text = "Remove Student", Dock = DockStyle.Top, BackColor = Color.Salmon)
let btnShowAllStudents = new Button(Text = "Show All Students", Dock = DockStyle.Top, BackColor = Color.LightYellow)

adminPanel.Controls.AddRange([| 
    lblID; txtID
    lblName; txtName
    lblGrades; txtGrades
    btnAdd; btnEdit; btnRemove; btnShowAllStudents
|])

// Viewer Panel (for viewing students and statistics)
let viewerPanel = new Panel(Dock = DockStyle.Fill, BackColor = Color.FromArgb(245, 245, 245))
let lstView = new ListBox(Dock = DockStyle.Top, Height = 200, BackColor = Color.WhiteSmoke)
let statisticsLabel = new Label(Dock = DockStyle.Bottom, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.DarkGreen)
let dataGridView = new DataGridView(Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.WhiteSmoke)
viewerPanel.Controls.AddRange([| lstView; statisticsLabel; dataGridView |])

// Login Panel (for switching between Admin and Viewer)
let loginPanel = new Panel(Dock = DockStyle.Top, Height = 100, BackColor = Color.LightCyan)
let txtUsername = new TextBox(PlaceholderText = "Username", Dock = DockStyle.Top)
let txtPassword = new TextBox(PlaceholderText = "Password", PasswordChar = '*', Dock = DockStyle.Top)
let btnLogin = new Button(Text = "Login", Dock = DockStyle.Top, BackColor = Color.LightSkyBlue)
loginPanel.Controls.AddRange([| txtUsername; txtPassword; btnLogin |])

// Add the login panel initially
mainPanel.Controls.Add(loginPanel)

// Update viewer panel with student information and class statistics
let updateViewerPanel() =
    lstView.Items.Clear()
    students |> List.iter (fun s -> 
        let avg = calculateAverage s.Grades
        lstView.Items.Add(sprintf "ID: %d, Name: %s, Avg: %.2f" s.ID s.Name avg) |> ignore
    )
    
    let avg, high, low, passRate = getClassStatistics students
    statisticsLabel.Text <- sprintf "Class Avg: %.2f, High: %d, Low: %d, Pass Rate: %.2f%%" avg high low passRate
    
    let dataSource = students |> List.map (fun s -> 
        [| box s.ID; box s.Name; box (calculateAverage s.Grades) |]) |> Array.ofList
    dataGridView.DataSource <- null
    dataGridView.DataSource <- dataSource

// Button Click Events for Admin CRUD Operations
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
            txtID.Clear()
            txtName.Clear()
            txtGrades.Clear()
        with
        | _ -> MessageBox.Show("Invalid input. Please check the fields.") |> ignore
    else
        MessageBox.Show("You do not have permission to add students.") |> ignore
)

btnEdit.Click.Add(fun _ -> 
    if isAdmin then
        try
            let id = int txtID.Text
            let name = txtName.Text
            let grades = txtGrades.Text.Split(',') |> Array.map int |> Array.toList
            students <- students |> List.map (fun s -> if s.ID = id then { s with Name = name; Grades = grades } else s)
            MessageBox.Show("Student updated successfully!") |> ignore
            updateViewerPanel()
            saveData()
        with
        | _ -> MessageBox.Show("Invalid input. Please check the fields.") |> ignore
    else
        MessageBox.Show("You do not have permission to edit students.") |> ignore
)

btnRemove.Click.Add(fun _ -> 
    if isAdmin then
        try
            let id = int txtID.Text
            students <- students |> List.filter (fun s -> s.ID <> id)
            MessageBox.Show("Student removed successfully!") |> ignore
            updateViewerPanel()
            saveData()
        with
        | _ -> MessageBox.Show("Invalid input. Please check the fields.") |> ignore
    else
        MessageBox.Show("You do not have permission to remove students.") |> ignore
)

btnShowAllStudents.Click.Add(fun _ ->
    if isAdmin then
        updateViewerPanel()
        mainPanel.Controls.Clear()
        mainPanel.Controls.Add(viewerPanel)
    else
        MessageBox.Show("You do not have permission to view all students.") |> ignore
)

// Button Click Event for Login
btnLogin.Click.Add(fun _ -> 
    if txtUsername.Text = "admin" && txtPassword.Text = "admin" then
        isAdmin <- true
        MessageBox.Show("Admin Login Successful!") |> ignore
    else
        isAdmin <- false
        MessageBox.Show("Viewer Login Successful!") |> ignore

    mainPanel.Controls.Clear()
    if isAdmin then
        mainPanel.Controls.Add(adminPanel)
    else
        updateViewerPanel()
        mainPanel.Controls.Add(viewerPanel)
)

// Load data and start application
loadData()
Application.Run(form)
