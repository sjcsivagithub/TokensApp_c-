# Beginner's Step-by-Step Guide: Debugging TokensApp

This guide will teach you exactly how to run the `TokensApp` project, how to start debugging using the **F5** key, where to place your breakpoints to understand the code flow, and how to test your API practically.

---

## Step 1: Open the Project in Your Editor
1. Open **Visual Studio** (recommended for beginners) or **Visual Studio Code (VS Code)**.
2. Go to **File > Open Folder** (or Open Project/Solution) and select this exact folder: 
   `c:\Users\sivaj\real_token_app\TokensApp`

---

## Step 2: Set Breakpoints (Pausing the Code)
A **breakpoint** is a red dot you place on a line of code. When the program reaches that line, it **pauses** so you can see exactly what data is flowing through it.

1. In the file explorer on the left, open `Controller/TokensController.cs`.
2. Scroll to **Line 18**: `public async Task<IActionResult> GetCategories()`.
3. Click on the gray margin to the far left of line 18. A **Red Dot** (breakpoint) will appear.
4. Scroll to **Line 31**: `public async Task<IActionResult> GetVendors(...)`.
5. Place another **Red Dot** on line 31.
6. Scroll to **Line 79**: `public async Task<IActionResult> BookAppointment(...)`.
7. Place one more **Red Dot** on line 79.

---

## Step 3: Start Debugging (Press F5)
1. On your keyboard, press the **F5** key. 
   *(Alternatively, in Visual Studio, click the green "Play" button at the top that says "TokensApp").*
2. **What happens next?**
   - The code compiles.
   - The application starts running.
   - A Command Prompt (black screen) might open showing `"Now listening on: http://localhost:5241"`.
   - Your web browser will automatically open a page called **Swagger** (usually at `http://localhost:5241/swagger`). 
   *(Swagger is a UI built into your project that lets you test APIs easily without Postman!)*

---

## Step 4: Test the Flow & See the Breakpoint in Action

Now we will test an API and watch the code pause!

### 1. Test "Categories" API
1. In the **Swagger** page in your browser, find the blue block labeled `GET /api/Tokens/categories`. Click it to expand it.
2. Click the **"Try it out"** button on the right.
3. Click the big blue **"Execute"** button.
4. **WATCH YOUR EDITOR!** Visual Studio will start blinking.
5. Go to Visual Studio. You will see a **Yellow Arrow** on the Red Dot you placed on Line 18 in `TokensController.cs`.
   - *This means the API call reached your C# code!*
6. **Step Through the Code:**
   - Press **F10** (Step Over). The yellow arrow moves down one line to `var cats = await _db.VendorCategories...`.
   - Hover your mouse over `_db` (AppDbContext) to see that the database is connected.
   - Press **F10** again. Now hover your mouse over `cats`. You will see the list of categories pulled from PostgreSQL!
7. Press **F5** (Continue) to let the code finish. 
8. Go back to your browser. You will see a `200 Success` code with the categories listed (Saloon, Gym, etc.).

### 2. Test "Vendors" API
1. In Swagger, expand `GET /api/Tokens/vendors`.
2. Click **"Try it out"**. You can leave the `categoryId` and `city` blank. Click **"Execute"**.
3. Visual Studio will pause on **Line 31**. 
4. Press **F10** to walk through the lines. You can hover over `categoryId` and `city` to see that they are `null` (because we left them blank in Swagger).
5. Watch how the code skips the `if (categoryId.HasValue)` condition because it's null!
6. Press **F5** to finish. Your browser shows the list of Vendors.

---

## Step 5: Fixing the Missing Tables (The 500 Error)
If you try to test `services` or `appointments`, you will likely face a `500 Internal Server Error`. Our debugging showed this is because the table `vendor_services` and `appointments` are missing in PostgreSQL.

**How to Fix This Flow:**
1. Stop debugging (press **Shift + F5** or click the red Stop square block in Visual Studio).
2. Open the **Package Manager Console** (View > Other Windows > Package Manager Console) OR a regular Terminal in VS Code.
3. Run this command to tell Entity Framework to write a script for creating the tables:
   ```bash
   dotnet ef migrations add InitialCreate
   ```
4. Run this command to actually build those tables in PostgreSQL:
   ```bash
   dotnet ef database update
   ```
5. Once that completes, press **F5** again, and you can now test booking appointments successfully!

---

## Summary of the Full Flow
1. **User Action:** Clicks "Execute" in Swagger (or Postman).
2. **Routing:** The request goes to `http://localhost:5241/api/Tokens/categories`. Because of the `[Route("api/[controller]")]` tag on your `TokensController.cs`, it knows to send the request there.
3. **Controller Method:** It matches the `[HttpGet("categories")]` tag, so it enters the `GetCategories()` method.
4. **Database (EF Core):** The method uses `_db` (`AppDbContext`) to query PostgreSQL.
5. **Response:** It returns an `Ok(cats)` which converts the C# list into JSON for Swagger to display.
