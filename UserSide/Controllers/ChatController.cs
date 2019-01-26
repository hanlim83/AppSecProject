using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using UserSide.Data;
using UserSide.Hubs;
using UserSide.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace UserSide.Controllers
{
    public class ChatController : Controller
    {
        //HUB
        private readonly ChatContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;





        public ChatController(ChatContext context, UserManager<IdentityUser> userManager, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        //private readonly IHubContext<ChatHub> _hubContext;

        //public ChatController(IHubContext<ChatHub> hubContext)
        //{
        //    _hubContext = hubContext;
        //}

        //public IHubContext<ChatHub, IChatClient> _strongChatHubContext { get; }

        //public ChatController(IHubContext<ChatHub, IChatClient> chatHubContext)
        //{
        //    _strongChatHubContext = chatHubContext;
        //}


        [Authorize]
        public async Task<IActionResult> Index(Chat chat)
        {
            // await _hubContext.Clients.All.SendAsync("Notify", $"Home page loaded at: {DateTime.Now}");
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var username = user.UserName;
            //string CurrUser = HttpContext.User.FindFirstValue(ClaimTypes.Name);
            //Chat c = new Chat();
            ////sort or fliter here


            //var item = _context.Chats.Where(x => x.UserOne == CurrUser)
            //    .Select(x => new Chat
            //    {
            //        UserTwo = x.UserTwo
            //    });

            //ViewBag.related = item;
          

            await _hubContext.Clients.All.SendAsync("Notify", $"Home page loaded at: {DateTime.Now}");
            return View(await _context.Chats.ToListAsync());

        }

        [Authorize]
        public async Task<IActionResult> TalkView()
        {
            //ViewBag.Dude = chat.ChatID;
            
            return View(await _context.Messages.ToListAsync());
        }

        public async Task<IActionResult> CreateGroup()
        {
            //take in user object
            var user = await _userManager.GetUserAsync(HttpContext.User);
            //For username (can use it inside method also)
            var username = user.UserName;


            return View(username);
        }

        public IActionResult CreateChat()
        {
            //var contact = new UserChat<string>();

            //foreach (var username in user)
            //{
            //    //for username (can use it inside method also)
            //    //var username = user.username;
            //    contact.add(username);
            //}
            //need to remove showing current user
            var uList = _userManager.Users.ToListAsync();

            var dictionary = new Dictionary<string, string>
            {

            };
            foreach (var user in uList.Result)
            {
                dictionary.Add(user.Id, user.UserName);
            }
            ViewBag.SelectList = new SelectList(dictionary, "Key", "Value");

            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateChat([Bind("ChatID,MsgCount,UserOne")] Chat chat)
        {
            if (ModelState.IsValid)
            {
                var User = await _userManager.GetUserAsync(HttpContext.User);
                var selectUser = await _userManager.Users.ToListAsync();

                //foreach (var u in selectUser)
                //{
                //    if (u.Id == chat.UserOne)
                //    {
                //        chat.UserOne = u.UserName;
                //    }
                //}


                chat.UserTwo = User.Id;

                _context.Add(chat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(chat);
        }

       
    }


   
}









    