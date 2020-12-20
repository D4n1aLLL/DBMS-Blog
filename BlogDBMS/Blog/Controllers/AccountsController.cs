using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Blog.Models;

namespace Blog.Controllers
{
    public class AccountsController : Controller
    {
        private readonly BlogModel _model = new BlogModel();

        // GET: Accounts
        //[HttpGet]
        //public ActionResult Login()
        //{
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        RedirectToAction("Index", "Posts");
        //    }
        //    return View("Login");
        //}
        //[HttpPost]
        public ActionResult Login(tblUser user)
        {
            if (Session["UserID"] != null)
                return RedirectToAction("UserDashBoard");
            if (ModelState.IsValid)
            {
                var obj = _model.tblUsers.FirstOrDefault(
                    a => a.username.Equals(user.username) && a.password.Equals(user.password));
                if (obj != null)
                {
                    if (obj.powerLevel == 0)
                    {
                        Session["IsAdmin"] = true;
                        Session["IsAuthor"] = null;
                        Session["IsUser"] = null;
                        Session["UserID"] = obj.ID;
                    }
                    else if (obj.powerLevel == 1)
                    {
                        Session["IsAdmin"] = null;
                        Session["IsAuthor"] = true;
                        Session["IsUser"] = null;
                        Session["UserID"] = obj.ID;
                    }
                    else if (obj.powerLevel == 2)
                    {
                        Session["IsAdmin"] = null;
                        Session["IsAuthor"] = null;
                        Session["IsUser"] = true;
                        Session["UserID"] = obj.ID;
                    }
                    else
                    {
                        Session["IsAdmin"] = null;
                        Session["IsAuthor"] = null;
                        Session["IsUser"] = null;
                        Session["UserID"] = null;
                        return View("Login");
                    }
                    TempData["Success"] = "Successfully logged in.";
                    return RedirectToAction("Index","Posts");
                }
            }
            return View(user);
        }

        public ActionResult UserDashBoard()
        {
            if (Session["UserID"] != null)
            {
                int userID = Convert.ToInt32(Session["UserID"].ToString());
                return View(_model.tblUsers.FirstOrDefault(a => a.ID == userID));
            }
            return RedirectToAction("Login");
        }
        [HttpPost]
        public ActionResult UserDashBoard(tblUser user)
        {
            if (Session["UserID"] != null)
            {
                if (ModelState.IsValid)
                {
                    tblUser dbUser = _model.tblUsers.SingleOrDefault(x => x.ID == user.ID);
                    if (user.ImageFile != null) { 
                    string fileName = Path.GetFileNameWithoutExtension(user.ImageFile.FileName);
                    string extension = Path.GetExtension(user.ImageFile.FileName);
                    fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                    user.profilePic = "~/Content/Images/" + fileName;
                    fileName = Path.Combine(Server.MapPath("~/Content/Images/"),fileName);
                    user.ImageFile.SaveAs(fileName);
                        dbUser.profilePic = user.profilePic;
                    }
                    
                    dbUser.password = user.password;
                    dbUser.firstName = user.firstName;
                    dbUser.lastName = user.lastName;
                    _model.SaveChanges();
                    TempData["Success"] = "Data saved successfully.";
                }
                return RedirectToAction("UserDashBoard");
            }
            return RedirectToAction("UserDashBoard");
        }
        [HttpPost]
        public ActionResult Register([Bind(Exclude = "ID")]tblUser newUser)
        {
            if (Session["UserID"] != null)
            {
                RedirectToAction("UserDashBoard");
            }
            if (ModelState.IsValid)
            {
                if (_model.tblUsers.FirstOrDefault(x => x.username == newUser.username) == null)
                {
                    newUser.powerLevel = 2;
                    _model.tblUsers.Add(newUser);
                    _model.SaveChanges();
                    TempData["RegisterSuccess"] = "Registration successfull please log in.";
                    return RedirectToAction("Login");
                }
                else
                {
                    string error = "User " + newUser.username + " already exists.";
                    ModelState.AddModelError("", error);
                    return View();
                }
            }
            return View();
        }
        [HttpGet]
        public ActionResult Register()
        {
            if (Session["UserID"] != null)
            {
                return RedirectToAction("UserDashBoard");
            }
            return View();
        }
        public ActionResult Logout()
        {
            Session["IsAdmin"] = null;
            Session["IsAuthor"] = null;
            Session["IsUser"] = null;
            Session["UserID"] = null;
            return RedirectToAction("Index", "Posts");
        }

        public ActionResult Index()
        {
            if (Session["UserID"] != null)
            {
                return RedirectToAction("UserDashBoard");
            }
            return View("Login");
        }
    }
}