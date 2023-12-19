using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeTimeSheetAPI;


// Create a WepApplicationBuilder and a WebApplication
// With preconfigured defaults
var builder = WebApplication.CreateBuilder(args);                           // [1] & [2] & [3]

//var app = builder.Build();                                                // [1] & [2] use, but [3] can't use this statement
                                                                          //var conString = builder.Configuration.GetConnectionString("EmployeeDb") ?? "Data Source=TimeSheet.db"; //[3] use this statement, but [1],[2] can use this statement

// Add database context to the dependency injection (DI) container
// builder.Services.AddSqlite<EmployeeTimeSheetDb>(conString);              // [1] & [2] use, but [3] can't use this statement      
builder.Services.AddDbContext<EmployeeTimeSheetDb>(opt => opt.UseInMemoryDatabase("EmployeeList"));    //[3] use this statement, but [1],[2] can use this statement
// Error CS1061	'DbContextOptionsBuilder' does not contain a definition for 'UseInMemoryDatabase' and no accessible extension method 'UseInMemoryDatabase' accepting a first argument of type 'DbContextOptionsBuilder' could be found (are you missing a using directive or an assembly reference?)	EmployeeTimeSheetAPI D:\2.Norton - University\Year II, Semester2, Academic Software Development\2. C# programming\4 Project C#\4-WebAPI\Program.cs	16	Active [How to fix : install : Install-Package Microsoft.EntityFrameworkCore.InMemory in NuGet Package Manager in Visual Studio ]

//Add Swagger Service
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// Enable the middleware for serving the generated JSON document and Swagger UI
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

// Enable the swagger UI in development mode only
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var employeeLists = new List<Employee>                                  // [1] & [2]
{
    new Employee() {Id=1,Name ="Java"},
    new Employee() {Id=2,Name ="Python"},
    new Employee() {Id=3,Name ="Pro.Json"},
    new Employee() {Id=4,Name ="C#"},
    new Employee() {Id=5,Name ="MySQL"},
    new Employee() {Id=6,Name ="REST AIP"}
};
// Create a group URL
//   ----- [1] ----- Don't Create URL (Work in C# commmand)
var empItems = app.MapGroup("/employees");                              // [2] & [3]
empItems.MapGet("/", GetAllEmployees);
empItems.MapGet("/{id}", GetEmployeeById);
empItems.MapPost("/", CreateEmployee);
empItems.MapPut("/", UpdateEmployee);
empItems.MapDelete("/{id}", DeleteEmployee);
app.Run();
// Read All Employee ########################################################################################################
/*app.MapGet("/employees", () =>                                                                      // [1]
{
    return employeeLists;
});
IResult GetAllEmployees()                                                                           // [2]
{
    return TypedResults.Ok(employeeLists);
}*/
static async Task<IResult> GetAllEmployees(EmployeeTimeSheetDb db)                                   // [3]
{
    return TypedResults.Ok(await db.Employees.ToArrayAsync());
}

// Read employee by id ######################################################################################################
/*app.MapGet("/employees/{id}", (int id) =>                                                         // [1]
{
    var employees = employeeLists.Find(s => s.Id == id);
    return employees == null ? Results.NotFound() : Results.Ok(employees);
});
IResult GetEmployeeById(int id)                                                                     // [2]
{
    var emp = employeeLists.Find(s => s.Id == id);
    return emp == null ? TypedResults.NotFound() : TypedResults.Ok(emp);
}*/
static async Task<IResult> GetEmployeeById(int id, EmployeeTimeSheetDb db)                       // [3]
{
    var emp = await db.Employees.FindAsync(id);
    return emp == null ? TypedResults.NotFound() : TypedResults.Ok(emp);
}

// Add new Employee ########################################################################################################
/*app.MapPost("/employees", ([FromBody] Employee inputEmp) =>                                       // [1]
{
    employeeLists.Add(inputEmp);
    return Results.Created($"/employees/{inputEmp.Id}", inputEmp);
});
IResult CreateEmployee([FromBody] Employee inputEmp)                                                // [2]
{
    employeeLists.Add(inputEmp);
    return TypedResults.Created($"/employees/{inputEmp.Id}", inputEmp);
}*/
static async Task<IResult> CreateEmployee([FromBody] Employee inputEmp, EmployeeTimeSheetDb db)     // [3]
{
    db.Employees.Add(inputEmp);
    await db.SaveChangesAsync();
    return TypedResults.Created($"/employees/{inputEmp.Id}", inputEmp);
}

// Update the employees ####################################################################################################
/*app.MapPut("/employees", ([FromBody] Employee inputEmp) =>                                          // [1]
{
    if (inputEmp is null) return Results.NotFound();

    int foundIndex = employeeLists.FindIndex(s => s.Id == inputEmp.Id);
    if (foundIndex < 0) return Results.NotFound();

    employeeLists[foundIndex] = inputEmp;
    return Results.NoContent();
});
IResult UpdateEmployee([FromBody] Employee inputEmp)                                                // [2]
{
    if (inputEmp is null) return TypedResults.NotFound();

    int foundIndex = employeeLists.FindIndex(s => s.Id == inputEmp.Id);
    if (foundIndex < 0) return TypedResults.NotFound();

    employeeLists[foundIndex] = inputEmp;
    return TypedResults.NoContent();
}*/
static async Task<IResult> UpdateEmployee([FromBody] Employee inputEmp, EmployeeTimeSheetDb db)     // [3]
{
    if (inputEmp is null) return TypedResults.NotFound();

    var emp = await db.Employees.FindAsync(inputEmp.Id);
    if (emp is null) return TypedResults.NotFound();

    emp.Name = inputEmp.Name;// Update student name
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
}

// Delect the employee   ####################################################################################################                                                                          
/*app.MapDelete("/employees/{id}", (int? id) =>                                                       // [1]
{
    if (id is null) return Results.NotFound();

    int foundIndex = employeeLists.FindIndex((s) => s.Id == id);
    if (foundIndex < 0) return Results.NotFound();

    employeeLists.RemoveAt(foundIndex);
    return Results.NoContent();
});
IResult DeleteEmployee(int? id)                                                                     // [2]
{
    if (id is null) return TypedResults.NotFound();

    int foundIndex = employeeLists.FindIndex(s => s.Id == id);
    if (foundIndex > 0) return TypedResults.NotFound();

    employeeLists.RemoveAt(foundIndex);
    return TypedResults.Ok(id);
}*/
static async Task<IResult> DeleteEmployee(int? id, EmployeeTimeSheetDb db)                          // [3]
{
    if (id is null) return TypedResults.NotFound();

    var emp = await db.Employees.FindAsync(id);
    if (emp is null) return TypedResults.NotFound();

    db.Employees.Remove(emp);
    await db.SaveChangesAsync();
    return TypedResults.Ok(id);
}
//Run the REST API Server at port : ---change in launchSetting.json
app.Run();