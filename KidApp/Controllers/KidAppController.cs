using KidApp.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace KidApp.Controllers
{
    public class Status
    {
        public int code { get; set; }
        public string message { get; set; }
        public Status(int code, string message)
        {
            this.code = code;
            this.message = message;
        }

        public Status()
        {
        }
    }

    public class ObjectResponse
    {
        public Status status = new Status(200, "Success");
        public Dictionary<String, dynamic> data { get; set; }
    }

    public class KidAppController : ApiController
    {

        KidAppEntities db = new KidAppEntities();

        private HttpResponseMessage ErrorResponseMessage(string message)
        {
            ObjectResponse response = new ObjectResponse();
            response.status = new Status(0, message);
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        private string GetUsernameById(string id)
        {
            var user = from us in db.Users
                       where us.userId == id
                       select us;
            if (user.Count() == 0) return null;
            return user.First().userName;
        }

        private bool ExistedUser(string username)
        {
            var user = from us in db.Users
                       where us.userName == username
                       select us;
            if (user.Count() == 0) return false;
            return true;
        }

        [Route("login")]
        public HttpResponseMessage PostLogin()
        {
            try
            {
                var username = System.Web.HttpContext.Current.Request.Params["username"];
                var password = System.Web.HttpContext.Current.Request.Params["password"];

                //byte[] data = System.Text.Encoding.ASCII.GetBytes(password);
                //data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
                //String passwordHash = System.Text.Encoding.ASCII.GetString(data);

                var user = from us in db.Users
                           where us.userName == username && us.password == password
                           select us;
                if (user.Count() == 0)
                {
                    return ErrorResponseMessage("Sai tên đăng nhập hoặc mật khẩu");
                }
                else if (user.First().active == false)
                {
                    return ErrorResponseMessage("Tài khoản đã bị vô hiệu hóa. Vui lòng sử dụng tài khoản khác.");
                }
                else
                {
                    ObjectResponse response = new ObjectResponse
                    {
                        status = new Status(200, "Đăng nhập thành công"),
                        data = new Dictionary<string, dynamic>()
                    };
                    response.data.Add("username", user.First().userName);
                    response.data.Add("date_create", user.First().dob);
                    response.data.Add("address", user.First().address);
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                return ErrorResponseMessage("Có lỗi xảy ra. Vui lòng thử lại sau.");
            }
        }

        [Route("register")]
        public HttpResponseMessage PostRegister()
        {
            try
            {
                var username = System.Web.HttpContext.Current.Request.Params["username"];
                var password = System.Web.HttpContext.Current.Request.Params["password"];
                var address = System.Web.HttpContext.Current.Request.Params["address"];

                if (!username.All(b => b < 128) || !password.All(b => b < 128)) // Check ASCII character.
                {
                    return ErrorResponseMessage("Tên đăng nhập hoặc mật khẩu không đúng định dạng.");
                }
                else if (username.Count() == 0 || password.Count() == 0 || address.Count() == 0) // Check enough information.
                {
                    return ErrorResponseMessage("Vui lòng nhập đầy đủ thông tin.");
                }
                else if (username.Length < 6 || password.Length < 6) // Check length of username and password.
                {
                    return ErrorResponseMessage("Tên đăng nhập và mật khẩu phải có độ dài ít nhất 6 ký tự");
                }
                else
                {
                    if (ExistedUser(username))
                    {
                        return ErrorResponseMessage("Tên đăng nhập đã tồn tại. Vui lòng chọn tên đăng nhập khác.");
                    }
                    //byte[] data = System.Text.Encoding.ASCII.GetBytes(password);
                    //data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
                    //String passwordHash = System.Text.Encoding.ASCII.GetString(data);

                    var lastUser = (from us in db.Users orderby us.userId descending select us.userId).First();
                    int p = int.Parse(lastUser.Substring(4)) + 1;
                    string id = "User" + p.ToString();
                    User user = new User
                    {
                        userId = id,
                        userName = username,
                        password = password,
                        address = address,
                        dob = DateTime.Now,
                        active = true
                    };
                    db.Users.Add(user);
                    db.SaveChanges();
                    ObjectResponse response = new ObjectResponse
                    {
                        status = new Status(200, "Đăng ký thành công"),
                        data = new Dictionary<string, dynamic>()
                    };
                    response.data.Add("username", user.userName);
                    response.data.Add("date_create", user.dob);
                    response.data.Add("address", user.address);
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception ex)
            {
                return ErrorResponseMessage("Có lỗi xảy ra. Vui lòng thử lại sau.");
            }
        }

        [Route("changePassword")]
        public HttpResponseMessage PutChangePassword()
        {
            try
            {
                var username = System.Web.HttpContext.Current.Request.Params["username"];
                var oldPassword = System.Web.HttpContext.Current.Request.Params["old_password"];
                var newPassword = System.Web.HttpContext.Current.Request.Params["new_password"];

                if (username.Count() == 0 || oldPassword.Count() == 0 || newPassword.Count() == 0)
                {
                    return ErrorResponseMessage("Vui lòng nhập đầy đủ thông tin.");
                }
                if (newPassword.Length < 6)
                {
                    return ErrorResponseMessage("Mật khẩu mới phải có ít nhất 6 ký tự.");
                }
                User user = db.Users.Single(us => us.userName == username);
                if (user.userId == null || user.active == false)
                {
                    return ErrorResponseMessage("Tài khoản không hợp lệ.");
                }
                //byte[] data = System.Text.Encoding.ASCII.GetBytes(oldPassword);
                //data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
                //String oldPasswordHash = System.Text.Encoding.ASCII.GetString(data);

                if (oldPassword != user.password)
                {
                    return ErrorResponseMessage("Sai mật khẩu xác nhận.");
                }

                //byte[] newData = System.Text.Encoding.ASCII.GetBytes(newPassword);
                //newData = new System.Security.Cryptography.SHA256Managed().ComputeHash(newData);
                //String newPasswordHash = System.Text.Encoding.ASCII.GetString(newData);

                user.password = newPassword;
                db.SaveChanges();
                ObjectResponse response = new ObjectResponse
                {
                    status = new Status(200, "Cập nhật mật khẩu thành công."),
                    data = new Dictionary<string, dynamic>()
                };
                response.data.Add("username", user.userName);
                response.data.Add("date_create", user.dob);
                response.data.Add("address", user.address);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception)
            {
                return ErrorResponseMessage("Có lỗi xảy ra. Vui lòng thử lại sau");
            }
        }

        [Route("getAllPicture")]
        public HttpResponseMessage GetAllPicture()
        {
            var username = System.Web.HttpContext.Current.Request.Params["username"];

            User user = db.Users.Single(us => us.userName == username);
            
            return Request.CreateResponse(HttpStatusCode.OK, user , "application/json");
        }
    }
}
