using KidApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                    response.data.Add("date_create", user.First().dateCreated);
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

                    //var lastUser = (from us in db.Users orderby us.userId descending select us.userId).First();
                    var last = (from us in db.Users select us.userId).Count();
                    //int p = int.Parse(lastUser.Substring(4)) + 1;
                    string id = "User" + (last + 1).ToString();
                    User user = new User
                    {
                        userId = id,
                        userName = username,
                        password = password,
                        address = address,
                        dateCreated = DateTime.Now,
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
                    response.data.Add("date_create", user.dateCreated);
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
                response.data.Add("date_create", user.dateCreated);
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

        [Route("getAllResultOfOneUser")]
        public HttpResponseMessage PostGetAllResultOfOneUser()
        {
            try
            {
                var username = System.Web.HttpContext.Current.Request.Params["username"];


                var user = from us in db.Users
                           where us.userName == username
                           select us;
                if (user.Count() == 0)
                {
                    return ErrorResponseMessage("Không tìm thấy người dùng");
                }
                var userId = user.First().userId;

                ObjectResponse response = new ObjectResponse
                {
                    status = new Status(200, "Lấy dữ liệu thành công"),
                    data = new Dictionary<string, dynamic>()
                };
                List<dynamic> list = new List<dynamic>();
                var imageList = from im in db.Images
                                where im.userId == userId && im.active == true 
                                orderby im.imageId descending
                                select im;
                foreach (Image image in imageList)
                {
                    Dictionary<String, dynamic> im = new Dictionary<string, dynamic>();
                    im.Add("image_id", image.imageId);
                    im.Add("image_name", image.imageName);
                    im.Add("time_shoot", image.timeShoot);
                    var engResult = from en in db.EngResults
                                    where en.engId == image.imageId && en.active == true
                                    select en;
                    if (engResult.Count() != 0)
                    {
                        Dictionary<String, dynamic> engSub = new Dictionary<string, dynamic>();
                        engSub.Add("eng_1", engResult.First().eng1);
                        engSub.Add("eng_2", engResult.First().eng2);
                        engSub.Add("eng_3", engResult.First().eng3);
                        im.Add("eng_sub", engSub);
                    }
                    var vieResult = from vi in db.VieResults
                                    where vi.vieId == image.imageId && vi.active == true
                                    select vi;
                    if (vieResult.Count() != 0)
                    {
                        Dictionary<String, dynamic> vieSub = new Dictionary<string, dynamic>();
                        vieSub.Add("vie_1", vieResult.First().vie1);
                        vieSub.Add("vie_2", vieResult.First().vie2);
                        vieSub.Add("vie_3", vieResult.First().vie3);
                        im.Add("vie_sub", vieSub);
                    }
                    list.Add(im);
                }
                response.data.Add("pictures", list);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return ErrorResponseMessage("Có lỗi xảy ra. Vui lòng thử lại sau.");
            }
        }

        [Route("removeImage")]
        public HttpResponseMessage DeleteRemoveImage()
        {
            try
            {
                var id = System.Web.HttpContext.Current.Request.Params["image_id"];

                var image = from im in db.Images
                            where im.imageId == id
                            select im;
                if (image.Count() == 0)
                {
                    return ErrorResponseMessage("Hình ảnh không tồn tại.");
                }
                image.First().active = false;
                db.SaveChanges();
                ObjectResponse response = new ObjectResponse
                {
                    status = new Status(200, "Xóa hình ảnh thành công"),
                    data = new Dictionary<string, dynamic>()
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return ErrorResponseMessage("Có lỗi xảy ra. Vui lòng thử lại sau.");
            }
        }

        public class ImageObject
        {
            [JsonProperty(PropertyName = "image_name")]
            public string name { get; set; }
            [JsonProperty(PropertyName = "image_time")]
            public double time { get; set; }
            [JsonProperty(PropertyName = "username")]
            public string username { get; set; }

            [JsonProperty(PropertyName = "eng_sub")]
            public Engsub engsub { get; set; }
            [JsonProperty(PropertyName = "vie_sub")]
            public Viesub vietsub { get; set; }
            public class Engsub
            {
                public string eng_1 { get; set; }
                public string eng_2 { get; set; }
                public string eng_3 { get; set; }
            }
            public class Viesub
            {
                public string vie_1 { get; set; }
                public string vie_2 { get; set; }
                public string vie_3 { get; set; }
            }
        }

        [Route("add-an-image")]
        public HttpResponseMessage PostAddImage(JObject requestParam)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject<ImageObject>(requestParam.ToString());
                //var lastImage = (from im in db.Images
                //               orderby im.imageId descending
                //             select im.imageId).First();
                var last = (from im in db.Images select im.imageId).Count();
                var user = from us in db.Users
                           where us.userName == obj.username
                           select us;
                int p = last + 1;
                string id = "Image" + p.ToString();
                if (user.Count() == 0)
                {
                    return ErrorResponseMessage("Ten dang nhap khong ton tai.");
                }
                Image image = new Image
                {
                    imageId = id,
                    imageName = obj.name,
                    timeShoot = obj.time,
                    userId = user.First().userId,
                    active = true
                };
                EngResult eng = new EngResult
                {
                    engId = id,
                    eng1 = obj.engsub.eng_1,
                    eng2 = obj.engsub.eng_2,
                    eng3 = obj.engsub.eng_3,
                    active = true,
                };
                VieResult vie = new VieResult
                {
                    vieId = id,
                    vie1 = obj.vietsub.vie_1,
                    vie2 = obj.vietsub.vie_2,
                    vie3 = obj.vietsub.vie_3,
                    active = true
                };
                db.Images.Add(image);
                db.SaveChanges();
                db.EngResults.Add(eng);
                db.VieResults.Add(vie);
                db.SaveChanges();
                ObjectResponse response = new ObjectResponse
                {
                    status = new Status(200, "Thêm vào thành công"),
                    data = new Dictionary<string, dynamic>()
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return ErrorResponseMessage("Có lỗi xảy ra. Vui lòng thử lại sau." + ex.Message);
            }
        }

    }
}
