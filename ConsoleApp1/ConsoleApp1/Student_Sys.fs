module Student_Sys

open System
open System.IO
open Newtonsoft.Json
open System.Windows.Forms

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
