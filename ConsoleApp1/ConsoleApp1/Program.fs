open System
open System.Windows.Forms
open System.Drawing

// Define student record
type Student = {
    ID: int
    Name: string
    Grades: list<int>
}

// Application state
let mutable students: list<Student> = []
let mutable isAdmin = false // Default role as Viewer

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
            let passCount = float (List.length passed)  // Convert length to float
            let totalCount = float (List.length allGrades)  // Convert length to float
            (passCount / totalCount) * 100.0  // Calculate pass rate as percentage
    
    (average, highest, lowest, passRate)


// GUI Elements
let form = new Form(Text = "Student Grades Management", Width = 800, Height = 600)

// Main Panel
let mainPanel = new Panel(Dock = DockStyle.Fill, BackColor = Color.LightGray)
form.Controls.Add(mainPanel)

// Admin Panel (for CRUD operations)
let adminPanel = new Panel(Dock = DockStyle.Top, Height = 200)
let txtID = new TextBox(PlaceholderText = "Student ID")
let txtName = new TextBox(PlaceholderText = "Student Name")
let txtGrades = new TextBox(PlaceholderText = "Grades (comma-separated)")
let btnAdd = new Button(Text = "Add Student", Dock = DockStyle.Top)
let btnEdit = new Button(Text = "Edit Student", Dock = DockStyle.Top)
let btnRemove = new Button(Text = "Remove Student", Dock = DockStyle.Top)
adminPanel.Controls.AddRange([| txtID; txtName; txtGrades; btnAdd; btnEdit; btnRemove |])

// Viewer Panel (for viewing students and statistics)
let viewerPanel = new Panel(Dock = DockStyle.Fill)
let lstView = new ListBox(Dock = DockStyle.Top, Height = 200)
let statisticsLabel = new Label(Dock = DockStyle.Bottom, TextAlign = ContentAlignment.MiddleCenter)
let dataGridView = new DataGridView(Dock = DockStyle.Fill)
viewerPanel.Controls.AddRange([| lstView; statisticsLabel; dataGridView |])

// Login Panel (for switching between Admin and Viewer)
let loginPanel = new Panel(Dock = DockStyle.Top, Height = 100)
let txtUsername = new TextBox(PlaceholderText = "Username", Dock = DockStyle.Top)
let txtPassword = new TextBox(PlaceholderText = "Password", PasswordChar = '*', Dock = DockStyle.Top)
let btnLogin = new Button(Text = "Login", Dock = DockStyle.Top)
loginPanel.Controls.AddRange([| txtUsername; txtPassword; btnLogin |])

// Add the login panel initially
mainPanel.Controls.Add(loginPanel)

// Button Click Events for Admin CRUD Operations
btnAdd.Click.Add(fun _ -> 
    if isAdmin then
        try
            let id = int txtID.Text
            let name = txtName.Text
            let grades = txtGrades.Text.Split(',') |> Array.map int |> Array.toList
            students <- { ID = id; Name = name; Grades = grades } :: students
            MessageBox.Show("Student added successfully!") |> ignore
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
            let studentToEdit = students |> List.tryFind (fun s -> s.ID = id)
            match studentToEdit with
            | Some student ->
                students <- students |> List.map (fun s -> 
                    if s.ID = student.ID then { s with Name = name; Grades = grades }
                    else s
                )
                MessageBox.Show("Student edited successfully!") |> ignore
            | None -> MessageBox.Show("Student not found!") |> ignore
        with
        | _ -> MessageBox.Show("Invalid input. Please check the fields.") |> ignore
    else
        MessageBox.Show("You do not have permission to edit students.") |> ignore
)

btnRemove.Click.Add(fun _ -> 
    if isAdmin then
        try
            let id = int txtID.Text
            let studentToRemove = students |> List.tryFind (fun s -> s.ID = id)
            match studentToRemove with
            | Some student ->
                students <- students |> List.filter (fun s -> s.ID <> student.ID)
                MessageBox.Show("Student removed successfully!") |> ignore
            | None -> MessageBox.Show("Student not found!") |> ignore
        with
        | _ -> MessageBox.Show("Invalid input. Please check the fields.") |> ignore
    else
        MessageBox.Show("You do not have permission to remove students.") |> ignore
)

// Update viewer panel with student information and class statistics
let updateViewerPanel() =
    lstView.Items.Clear()
    students |> List.iter (fun s -> 
        let avg = calculateAverage s.Grades
        lstView.Items.Add(sprintf "ID: %d, Name: %s, Avg: %.2f" s.ID s.Name avg) |> ignore
    )
    let avg, high, low, passRate = getClassStatistics students
    statisticsLabel.Text <- sprintf "Class Avg: %.2f, High: %d, Low: %d, Pass Rate: %.2f%%" avg high low passRate
    
    // Show students in a DataGridView table
    dataGridView.DataSource <- students |> List.map (fun s -> 
        [| box s.ID; box s.Name; box (calculateAverage s.Grades) |]) |> Array.ofList

// Button Click Event for Login
btnLogin.Click.Add(fun _ -> 
    // Simulate a login process
    if txtUsername.Text = "admin" && txtPassword.Text = "admin" then
        isAdmin <- true
        MessageBox.Show("Admin Login Successful!") |> ignore
    else
        isAdmin <- false
        MessageBox.Show("Viewer Login Successful!") |> ignore

    // Switch to the correct panel after login
    mainPanel.Controls.Clear()
    if isAdmin then
        mainPanel.Controls.Add(adminPanel)
    else
        updateViewerPanel()
        mainPanel.Controls.Add(viewerPanel)
)

// Start Application
Application.Run(form)
