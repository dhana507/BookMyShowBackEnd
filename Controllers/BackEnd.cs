using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Example.Models;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using static Example.Controllers.ToDoController;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
namespace Example.Controllers
{
    
    [ApiController]
    public class ToDoController : ControllerBase
    {
        private IConfiguration _configuration;
        public ToDoController(IConfiguration configuration) {
            _configuration = configuration;
        }
        private string GenerateJwtToken(string email, string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(ClaimTypes.Email, email),
        new Claim(ClaimTypes.Role, role)
    };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public class MovieDistrictRequest
        {
            public string? District { get; set; }

        }
        public class TheatermovietitleRequest
        {
            public string? movietitle { get; set; }

        }
        public class BookingRequest
        {
            public TheaterRequest details { get; set; }
            public List<string> setnumbers { get; set; }
            public string? totalamount { get; set; }
            public string? email { get; set; }
            public string? mobile { get; set; }
            public string? accountno { get; set; }
        }

        public class DeleteRequest
        {
            public string? theatername { get; set; }
            public string? movietitle { get; set; }
            public string? district { get; set; }

        }
       /* [HttpGet("get_tasks")]
        public JsonResult get_tasks()
        {
            string query = "select * from student";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("mydb");
            SqlDataReader myReader;
            using (SqlConnection mycon = new SqlConnection(sqlDataSource))
            {
                mycon.Open();
                using(SqlCommand mycommand =new SqlCommand(query, mycon))
                {
                    myReader = mycommand.ExecuteReader();
                    table.Load(myReader);
                   
                }
            }
            return new JsonResult(table);
        }
       */
        [HttpPost("register")]
        public JsonResult register([FromForm] RegisterRequest request)
        {
          
            string query = "insert into RegisterDetails values(@username,@email,@password)";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("mydb");
            SqlDataReader myReader;
            using (SqlConnection mycon = new SqlConnection(sqlDataSource))
            {
                mycon.Open();
                using (SqlCommand mycommand = new SqlCommand(query, mycon))
                {
                    mycommand.Parameters.AddWithValue("@username",request.username);
                    mycommand.Parameters.AddWithValue("@email", request.email);
                    mycommand.Parameters.AddWithValue("@password",request.password);
                    myReader = mycommand.ExecuteReader();
                    table.Load(myReader);

                }
            }
            var token = GenerateJwtToken(request.email, "User");
            return new JsonResult(new { Message = "User Successfully Login", Status = "Success", Token = token });

        }

     
        [HttpPost("alllogin")]
        public JsonResult Alllogin([FromForm] LoginRequest request)
        {
            string query = "SELECT * FROM AdminRegisterDetails WHERE email=@Email AND password=@Password";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("mydb");

            try
            {
                using (SqlConnection mycon = new SqlConnection(sqlDataSource))
                {
                    mycon.Open();
                    using (SqlCommand mycommand = new SqlCommand(query, mycon))
                    {
                        mycommand.Parameters.Add("@Email", SqlDbType.VarChar).Value = request.email;
                        mycommand.Parameters.Add("@Password", SqlDbType.VarChar).Value = request.password;

                        using (SqlDataReader myReader = mycommand.ExecuteReader())
                        {
                            table.Load(myReader);
                        }
                    }
                }

                if (table.Rows.Count > 0)
                {
                    var token = GenerateJwtToken(request.email, "Admin");
                    return new JsonResult(new { Message = "Admin Successfully Login", Status = "Success", Token = token });
                }
                else
                {
                    string query1 = "SELECT * FROM RegisterDetails WHERE email=@Email AND password=@Password";
                    DataTable table1 = new DataTable();

                    using (SqlConnection mycon = new SqlConnection(sqlDataSource))
                    {
                        mycon.Open();
                        using (SqlCommand mycommand1 = new SqlCommand(query1, mycon))
                        {
                            mycommand1.Parameters.Add("@Email", SqlDbType.VarChar).Value = request.email;
                            mycommand1.Parameters.Add("@Password", SqlDbType.VarChar).Value = request.password;

                            using (SqlDataReader myReader = mycommand1.ExecuteReader())
                            {
                                table1.Load(myReader);
                            }
                        }
                    }

                    if (table1.Rows.Count > 0)
                    {
                        var token = GenerateJwtToken(request.email, "User");
                        return new JsonResult(new { Message = "User Successfully Login", Status = "Success", Token = token });
                    }
                    else
                    {
                        return new JsonResult(new { Message = "Invalid Credentials", Status = "Failed" });
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { Message = "An error occurred", Error = ex.Message });
            }
        }


        [Authorize]
        [HttpPost("getmoviedetails")]
        public IActionResult GetMovieDetails([FromBody] MovieDistrictRequest request)
        {
            if (string.IsNullOrEmpty(request.District))
            {
                return BadRequest(new { error = "District is required." });
            }

            string query = "SELECT * FROM Movie WHERE district = @district";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("mydb");

            using (SqlConnection mycon = new SqlConnection(sqlDataSource))
            {
                mycon.Open();
                using (SqlCommand mycommand = new SqlCommand(query, mycon))
                {
                    mycommand.Parameters.AddWithValue("@district", request.District);
                    using (SqlDataReader myReader = mycommand.ExecuteReader())
                    {
                        table.Load(myReader);
                    }
                }
            }
            return Ok(table);
        }

        [Authorize]
        [HttpPost("gettheatredetails")]
        public IActionResult Gettheatredetails([FromBody] TheatermovietitleRequest request)
        {
            if (string.IsNullOrEmpty(request.movietitle))
            {
                return BadRequest(new { error = "moviename is required." });
            }

            string query = "SELECT * FROM Theater WHERE movietitle=@movietitle";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("mydb");

            using (SqlConnection mycon = new SqlConnection(sqlDataSource))
            {
                mycon.Open();
                using (SqlCommand mycommand = new SqlCommand(query, mycon))
                {
                    mycommand.Parameters.AddWithValue("@movietitle", request.movietitle);
                    using (SqlDataReader myReader = mycommand.ExecuteReader())
                    {
                        table.Load(myReader);
                    }
                }
            }
            return Ok(table);
        }

        [Authorize]
        [HttpPost("addtheater")]
        public JsonResult AddTheater([FromForm] TheaterRequest request)
        {
            string query = "INSERT INTO Theater (theatername, time, seats, price, allotseats, district, address, movietitle, date) " +
                           "VALUES (@theatername, @time, @seats, @price, @allotseats, @district, @address, @movietitle, @date)";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("mydb");
            SqlDataReader myReader;

            try
            {
                using (SqlConnection mycon = new SqlConnection(sqlDataSource))
                {
                    mycon.Open();
                    using (SqlCommand mycommand = new SqlCommand(query, mycon))
                    {
                        mycommand.Parameters.AddWithValue("@theatername", request.theatername);
                        mycommand.Parameters.AddWithValue("@time", request.time);
                        mycommand.Parameters.AddWithValue("@seats", request.seats);
                        mycommand.Parameters.AddWithValue("@price", request.price);
                      
                        mycommand.Parameters.AddWithValue("@allotseats", string.IsNullOrEmpty(request.allotseats) ? DBNull.Value : request.allotseats);
                        mycommand.Parameters.AddWithValue("@district", request.district);
                        mycommand.Parameters.AddWithValue("@address", request.address);
                        mycommand.Parameters.AddWithValue("@movietitle", request.movietitle);
                        mycommand.Parameters.AddWithValue("@date", request.date);

                        myReader = mycommand.ExecuteReader();
                        table.Load(myReader);
                    }
                }
                return new JsonResult(new { Message = "Successfully Added" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return new JsonResult(new { Message = ex.Message }); // Convert the exception to a string
            }
        }
        [Authorize]
        [HttpPost("addnewmovie")]
        public JsonResult Addnewmovie([FromForm] MovieRequest request)
        {
            string query = "INSERT INTO Movie (title,imagelink,imagelink2,trailervideolink,heroname,heroinename,district,herolink,heroinelink,directorname,directorlink,producername,producerlink,musicname,musiclink) " +
                           "VALUES (@title, @imagelink, @imagelink2, @trailervideolink, @heroname, @heroinename, @district, @herolink, @heroinelink,@directorname,@directorlink,@producername,@producerlink,@musicname,@musiclink)";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("mydb");
            SqlDataReader myReader;

            try
            {
                using (SqlConnection mycon = new SqlConnection(sqlDataSource))
                {
                    mycon.Open();
                    using (SqlCommand mycommand = new SqlCommand(query, mycon))
                    {
                        mycommand.Parameters.AddWithValue("@title", request.title);
                        mycommand.Parameters.AddWithValue("@imagelink", request.imagelink);
                        mycommand.Parameters.AddWithValue("@imagelink2", request.imagelink2);
                        mycommand.Parameters.AddWithValue("@trailervideolink", request.trailervideolink);
                        mycommand.Parameters.AddWithValue("@heroname", request.heroname);
                        mycommand.Parameters.AddWithValue("@heroinename", request.heroinename);
                        mycommand.Parameters.AddWithValue("@district", request.district);
                        mycommand.Parameters.AddWithValue("@herolink", request.herolink);
                        mycommand.Parameters.AddWithValue("@heroinelink", request.heroinelink);
                        mycommand.Parameters.AddWithValue("@directorname", request.directorname);
                        mycommand.Parameters.AddWithValue("@directorlink", request.directorlink);
                        mycommand.Parameters.AddWithValue("@producername", request.producername);
                        mycommand.Parameters.AddWithValue("@producerlink", request.producerlink);
                        mycommand.Parameters.AddWithValue("@musicname", request.musicname);
                        mycommand.Parameters.AddWithValue("@musiclink", request.musiclink);

                        myReader = mycommand.ExecuteReader();
                        table.Load(myReader);
                    }
                }
                return new JsonResult(new { Message = "Successfully Added" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return new JsonResult(new { Message = ex.Message }); // Convert the exception to a string
            }
        }

        [Authorize]
        [HttpPost("deletetheatre")]
        public JsonResult deletetheatre([FromForm] DeleteRequest request)
        {
            try
            {
                string query = "delete from Theater where theatername=@theatername and movietitle=@movietitle and district=@district";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("mydb");
            SqlDataReader myReader;
            using (SqlConnection mycon = new SqlConnection(sqlDataSource))
            {
                mycon.Open();
                using (SqlCommand mycommand = new SqlCommand(query, mycon))
                {
                    mycommand.Parameters.AddWithValue("@theatername",request.theatername);
                    mycommand.Parameters.AddWithValue("@movietitle", request.movietitle);
                    mycommand.Parameters.AddWithValue("@district", request.district);
                    myReader = mycommand.ExecuteReader();
                    table.Load(myReader);

                }
            }
            return new JsonResult(new { Message = "Successfully Deleted" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return new JsonResult(new { Message = ex.Message }); // Convert the exception to a string
            }
        }
        [Authorize]
        [HttpPost("deletemovie")]
        public JsonResult Deletemovie([FromForm] TheatermovietitleRequest request)
        {
            try
            {
                string query = "delete from Movie where title=@movietitle";
                DataTable table = new DataTable();
                string sqlDataSource = _configuration.GetConnectionString("mydb");
                SqlDataReader myReader;
                using (SqlConnection mycon = new SqlConnection(sqlDataSource))
                {
                    mycon.Open();
                    using (SqlCommand mycommand = new SqlCommand(query, mycon))
                    {
                        mycommand.Parameters.AddWithValue("@movietitle", request.movietitle);
                      
                        myReader = mycommand.ExecuteReader();
                        table.Load(myReader);

                    }
                }
                return new JsonResult(new { Message = "Successfully Deleted" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return new JsonResult(new { Message = ex.Message }); // Convert the exception to a string
            }
        }
        [Authorize]
        [HttpPost("bookticket")]
        public IActionResult BookingTicket([FromBody] BookingRequest bookingRequest)
        {
            if (bookingRequest == null || bookingRequest.details == null || bookingRequest.setnumbers == null)
            {
                return BadRequest(new { message = "Invalid booking request." });
            }

            string selectQuery = @"SELECT allotseats FROM Theater WHERE 
                            theatername = @theatername AND 
                            movietitle = @movietitle AND 
                            date = @date AND 
                            time = @time";

            string updateQuery = @"UPDATE Theater SET allotseats = @allotseats WHERE 
                            theatername = @theatername AND 
                            movietitle = @movietitle AND 
                            date = @date AND 
                            time = @time";

            string insertQuery = @"INSERT INTO Bookinghistory (movietitle, theatername, time, date, district, totalamount, email, mobile, accountno, seatno) 
                           VALUES (@movietitle, @theatername, @time, @date, @district, @totalamount, @email, @mobile, @accountno, @seatno)";

            string sqlDataSource = _configuration.GetConnectionString("mydb");

            using (SqlConnection connection = new SqlConnection(sqlDataSource))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Fetch existing booked seats
                        List<string> bookedSeats;
                        using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection, transaction))
                        {
                            selectCommand.Parameters.AddWithValue("@theatername", bookingRequest.details.theatername);
                            selectCommand.Parameters.AddWithValue("@movietitle", bookingRequest.details.movietitle);
                            selectCommand.Parameters.AddWithValue("@date", bookingRequest.details.date);
                            selectCommand.Parameters.AddWithValue("@time", bookingRequest.details.time);

                            using (SqlDataReader reader = selectCommand.ExecuteReader())
                            {
                                if (!reader.HasRows)
                                {
                                    return NotFound(new { message = "Theater not found." });
                                }

                                reader.Read();
                                bookedSeats = reader["allotseats"].ToString().Split(',').ToList();
                            }
                        }

                        // Check for conflicts
                        foreach (var seat in bookingRequest.setnumbers)
                        {
                            if (bookedSeats.Contains(seat))
                            {
                                return Conflict(new { message = $"Seat {seat} is already booked." });
                            }
                        }

                        // Update booked seats
                        bookedSeats.AddRange(bookingRequest.setnumbers);
                        string updatedSeats = string.Join(",", bookedSeats);

                        using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection, transaction))
                        {
                            updateCommand.Parameters.AddWithValue("@allotseats", updatedSeats);
                            updateCommand.Parameters.AddWithValue("@theatername", bookingRequest.details.theatername);
                            updateCommand.Parameters.AddWithValue("@movietitle", bookingRequest.details.movietitle);
                            updateCommand.Parameters.AddWithValue("@date", bookingRequest.details.date);
                            updateCommand.Parameters.AddWithValue("@time", bookingRequest.details.time);

                            updateCommand.ExecuteNonQuery();
                        }

                        // Insert into booking history
                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection, transaction))
                        {
                            insertCommand.Parameters.AddWithValue("@movietitle", bookingRequest.details.movietitle);
                            insertCommand.Parameters.AddWithValue("@theatername", bookingRequest.details.theatername);
                            insertCommand.Parameters.AddWithValue("@time", bookingRequest.details.time);
                            insertCommand.Parameters.AddWithValue("@date", bookingRequest.details.date);
                            insertCommand.Parameters.AddWithValue("@district", bookingRequest.details.district);
                            insertCommand.Parameters.AddWithValue("@totalamount", bookingRequest.totalamount);
                            insertCommand.Parameters.AddWithValue("@email", bookingRequest.email);
                            insertCommand.Parameters.AddWithValue("@mobile", bookingRequest.mobile);
                            insertCommand.Parameters.AddWithValue("@accountno", bookingRequest.accountno);
                            insertCommand.Parameters.AddWithValue("@seatno", string.Join(",", bookingRequest.setnumbers));

                            insertCommand.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return Ok(new { message = "Successfully booked" });
                    }
                    catch
                    {
                        transaction.Rollback();
                        return StatusCode(500, new { message = "Internal server error. Booking failed." });
                    }
                }
            }
        }


        /* [HttpPost("bookticket")]
         public IActionResult BookingTicket([FromBody] BookingRequest bookingRequest)
         {
             if (bookingRequest == null || bookingRequest.details == null || bookingRequest.setnumbers == null)
             {
                 return BadRequest(new { message = "Invalid booking request." });
             }

             string query = @"SELECT allotseats FROM Theater WHERE 
                      theatername = @theatername AND 
                      movietitle = @movietitle AND 
                      date = @date AND 
                      time = @time";

             DataTable table = new DataTable();
             string sqlDataSource = _configuration.GetConnectionString("mydb");

             using (SqlConnection connection = new SqlConnection(sqlDataSource))
             {
                 connection.Open();
                 using (SqlCommand command = new SqlCommand(query, connection))
                 {
                     command.Parameters.AddWithValue("@theatername", bookingRequest.details.theatername);
                     command.Parameters.AddWithValue("@movietitle", bookingRequest.details.movietitle);
                     command.Parameters.AddWithValue("@date", bookingRequest.details.date);
                     command.Parameters.AddWithValue("@time", bookingRequest.details.time);

                     using (SqlDataReader reader = command.ExecuteReader())
                     {
                         table.Load(reader);
                     }
                 }
             }

             if (table.Rows.Count == 0)
             {
                 return NotFound(new { message = "Theater not found." });
             }
             var bookedSeats = table.Rows[0]["allotseats"].ToString().Split(',').ToList();

             foreach (var seat in bookingRequest.setnumbers)
             {
                 if (bookedSeats.Contains(seat))
                 {
                     return Conflict(new { message = $"Seat {seat} is already booked." });
                 }
             }

             bookedSeats.AddRange(bookingRequest.setnumbers);
             string updatedSeats = string.Join(",", bookedSeats);

             string updateQuery = @"UPDATE Theater SET allotseats = @allotseats WHERE 
                           theatername = @theatername AND 
                           movietitle = @movietitle AND 
                           date = @date AND 
                           time = @time";

             using (SqlConnection connection = new SqlConnection(sqlDataSource))
             {
                 connection.Open();
                 using (SqlCommand command = new SqlCommand(updateQuery, connection))
                 {
                     command.Parameters.AddWithValue("@allotseats", updatedSeats);
                     command.Parameters.AddWithValue("@theatername", bookingRequest.details.theatername);
                     command.Parameters.AddWithValue("@movietitle", bookingRequest.details.movietitle);
                     command.Parameters.AddWithValue("@date", bookingRequest.details.date);
                     command.Parameters.AddWithValue("@time", bookingRequest.details.time);

                     command.ExecuteNonQuery();
                 }
             }
             string insertQuery = "insert into Bookinghistory values(@movietitle, @theatername, @time,@date,@district,@totalamount,@email,@mobile,@accountno,@seatno)";
             DataTable table1 = new DataTable();
             SqlDataReader myReader1;
             using (SqlConnection connection = new SqlConnection(sqlDataSource))
             {
                 connection.Open();
                 using (SqlCommand commandinsert = new SqlCommand(updateQuery, connection))
                 {
                     commandinsert.Parameters.AddWithValue("@movietitle", bookingRequest.details.movietitle);
                     commandinsert.Parameters.AddWithValue("@theatername", bookingRequest.details.theatername);
                     commandinsert.Parameters.AddWithValue("@time", bookingRequest.details.time);
                     commandinsert.Parameters.AddWithValue("@date", bookingRequest.details.date);
                     commandinsert.Parameters.AddWithValue("@district", bookingRequest.details.district);
                     commandinsert.Parameters.AddWithValue("@totalamount", bookingRequest.totalamount);
                     commandinsert.Parameters.AddWithValue("@email", bookingRequest.email);
                     commandinsert.Parameters.AddWithValue("@mobile", bookingRequest.mobile);
                     commandinsert.Parameters.AddWithValue("@accountno", bookingRequest.accountno);
                     commandinsert.Parameters.AddWithValue("@seatno", bookingRequest.setnumbers.ToString());
                     myReader1 = commandinsert.ExecuteReader();
                     table1.Load(myReader1);

                 }
             }
             return Ok(new { message = "Successfully booked" });
         }*/
        [Authorize]
        [HttpPost("getbookingtickets")]
        public IActionResult GetBookingTickets([FromBody] TheaterRequest newTheater)
        {
            if (newTheater == null)
            {
                return BadRequest(new { message = "Invalid booking request." });
            }

            string query = @"SELECT allotseats FROM Theater WHERE 
                     theatername = @theatername AND 
                     movietitle = @movietitle AND 
                     date = @date AND 
                     time = @time";

            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("mydb");

            using (SqlConnection connection = new SqlConnection(sqlDataSource))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@theatername", newTheater.theatername);
                    command.Parameters.AddWithValue("@movietitle", newTheater.movietitle);
                    command.Parameters.AddWithValue("@date", newTheater.date);
                    command.Parameters.AddWithValue("@time", newTheater.time);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        table.Load(reader);
                    }
                }
            }

            if (table.Rows.Count == 0)
            {
                return NotFound(new { message = "Theater not found." });
            }

            var bookedSeats = table.Rows[0]["allotseats"].ToString().Split(',').ToList();
            return Ok(new { message=bookedSeats });
        }

      
       
        [HttpPost("delete_task")]
        public JsonResult delete_task([FromForm] int sid)
        {
            string query = "delete from student where sid=@sid";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("mydb");
            SqlDataReader myReader;
            using (SqlConnection mycon = new SqlConnection(sqlDataSource))
            {
                mycon.Open();
                using (SqlCommand mycommand = new SqlCommand(query, mycon))
                {
                    mycommand.Parameters.AddWithValue("@sid", sid);
                    myReader = mycommand.ExecuteReader();
                    table.Load(myReader);

                }
            }
            return new JsonResult("Deleted Successfully");
        }

    }

}
