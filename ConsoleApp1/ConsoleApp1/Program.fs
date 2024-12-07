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
let adminPanel = new Panel(Dock = DockStyle.Fill, BackColor = Color.FromArgb(39, 174, 96), Padding = Padding(20))
let adminTitle = new Label(Text = "Admin Panel", Dock = DockStyle.Top, Font = new Font("Arial", 16.0F, FontStyle.Bold), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter)
let lblID = new Label(Text = "Student ID", ForeColor = Color.White, Dock = DockStyle.Top)
let txtID = new TextBox(PlaceholderText = "Enter Student ID", Dock = DockStyle.Top)
let lblName = new Label(Text = "Student Name", ForeColor = Color.White, Dock = DockStyle.Top)
let txtName = new TextBox(PlaceholderText = "Enter Student Name", Dock = DockStyle.Top)
let lblGrades = new Label(Text = "Grades (comma-separated)", ForeColor = Color.White, Dock = DockStyle.Top)
let txtGrades = new TextBox(PlaceholderText = "Enter Grades", Dock = DockStyle.Top)
let btnAdd = new Button(Text = "Add Student", Dock = DockStyle.Top, BackColor = Color.LimeGreen, ForeColor = Color.White)
let btnEdit = new Button(Text = "Edit Student", Dock = DockStyle.Top, BackColor = Color.DodgerBlue, ForeColor = Color.White)
let btnRemove = new Button(Text = "Remove Student", Dock = DockStyle.Top, BackColor = Color.Tomato, ForeColor = Color.White)
let btnShowAllStudents = new Button(Text = "Show All Students", Dock = DockStyle.Top, BackColor = Color.LightGoldenrodYellow, ForeColor = Color.Black)
let btnReturnToLogin = new Button(Text = "Return to Login", Dock = DockStyle.Bottom, BackColor = Color.LightSlateGray, ForeColor = Color.White)

adminPanel.Controls.AddRange([| 
    adminTitle; lblID; txtID
    lblName; txtName
    lblGrades; txtGrades
    btnAdd; btnEdit; btnRemove; btnShowAllStudents; btnReturnToLogin
|])

// Viewer Panel
let viewerPanel = new Panel(Dock = DockStyle.Fill, BackColor = Color.FromArgb(52, 73, 94), Padding = Padding(20))
let viewerTitle = new Label(Text = "Viewer Panel", Dock = DockStyle.Top, Font = new Font("Arial", 16.0F, FontStyle.Bold), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter)
let lstView = new ListBox(Dock = DockStyle.Top, Height = 250, BackColor = Color.WhiteSmoke)
let statisticsLabel = new Label(Dock = DockStyle.Bottom, Font = new Font("Arial", 12.0F, FontStyle.Italic), ForeColor = Color.White)
let btnBackToLoginViewer = new Button(Text = "Return to Login", Dock = DockStyle.Bottom, BackColor = Color.LightSlateGray, ForeColor = Color.White)

viewerPanel.Controls.AddRange([| viewerTitle; lstView; statisticsLabel; btnBackToLoginViewer |])

// Login Panel
let loginPanel = new Panel(Dock = DockStyle.Fill, BackColor = Color.FromArgb(41, 128, 185), Padding = Padding(20))
let loginTitle = new Label(Text = "Login", Dock = DockStyle.Top, Font = new Font("Arial", 20.0F, FontStyle.Bold), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter)
let txtUsername = new TextBox(PlaceholderText = "Username", Dock = DockStyle.Top)
let txtPassword = new TextBox(PlaceholderText = "Password", PasswordChar = '*', Dock = DockStyle.Top)
let btnLogin = new Button(Text = "Login", Dock = DockStyle.Top, BackColor = Color.MediumSeaGreen, ForeColor = Color.White)

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
                MessageBox.Show("Student details updated!") |> ignore
                updateViewerPanel()
                saveData()
            | None ->
                MessageBox.Show("No student found with the given ID.") |> ignore
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

// Ensure login logic works
btnLogin.Click.Add(fun _ -> 
    if txtUsername.Text = "admin" && txtPassword.Text = "admin" then
        isAdmin <- true
        mainPanel.Controls.Clear()
        mainPanel.Controls.Add(adminPanel)
    else
        isAdmin <- false
        mainPanel.Controls.Clear()
        updateViewerPanel()
        mainPanel.Controls.Add(viewerPanel)
)

btnReturnToLogin.Click.Add(fun _ -> 
    mainPanel.Controls.Clear()
    mainPanel.Controls.Add(loginPanel)
)

btnBackToLoginViewer.Click.Add(fun _ -> 
    mainPanel.Controls.Clear()
    mainPanel.Controls.Add(loginPanel)
)

loadData() // Load existing students
Application.Run(form)
