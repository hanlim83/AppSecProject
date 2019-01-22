using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using UserSide.Data;
using UserSide.Hubs;


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

        //public async Task SendMessage(string message)
        //{
        //    await _strongChatHubContext.Clients.All.ReceiveMessage(message);
        //}

        [Authorize]
        public async Task<IActionResult> Index()
        {
            // await _hubContext.Clients.All.SendAsync("Notify", $"Home page loaded at: {DateTime.Now}");

            //take in user object
            var user = await _userManager.GetUserAsync(HttpContext.User);
            //For username (can use it inside method also)
            var username = user.UserName;

            await _hubContext.Clients.All.SendAsync("Notify", $"Home page loaded at: {DateTime.Now}");
            return View();


        }

        [Authorize]
        public IActionResult TalkView()
        {
            return View();
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
            //take in user object



            //var user = 

            //var contact = new UserChat<string>();

            //foreach (var username in user)
            //{
            //    //for username (can use it inside method also)
            //    //var username = user.username;
            //    contact.add(username);
            //}
            var users = _userManager.Users.Select(u => u.UserName).ToList();
            ViewBag.Users = users;

            return View();
        }
    }


    //POST: UserChats/Create
    //To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task <IActionResult> CreateChat([Bind("username")] Message message)
    //{
    //    if (ModelState.isVaild()
    //    {
    //        _context.add(chat);
    //        await _context.savechangesasync();
    //        return redirecttoaction(nameof(index));
    //    }
    //    return view(Chat);
    //}



    //public class UserChatsController : Controller
    //{
    //    private readonly ChatContext _context;

    //    public UserChatsController(ChatContext context)
    //    {
    //        _context = context;
    //    }

    //    // GET: UserChats
    //    public async Task<IActionResult> Index()
    //    {
    //        return View(await _context.UserChats.ToListAsync());
    //    }

    //    // GET: UserChats/Details/5
    //    public async Task<IActionResult> Details(string id)
    //    {
    //        if (id == null)
    //        {
    //            return NotFound();
    //        }

    //        var userChat = await _context.UserChats
    //            .FirstOrDefaultAsync(m => m.UserID == id);
    //        if (userChat == null)
    //        {
    //            return NotFound();
    //        }

    //        return View(userChat);
    //    }

    //    // GET: UserChats/Create
    //    public IActionResult Create()
    //    {
    //        return View();
    //    }

    //    // POST: UserChats/Create
    //    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    //    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public async Task<IActionResult> Create([Bind("UserID,UserName,GroupID")] UserChat userChat)
    //    {
    //        if (ModelState.IsValid)
    //        {
    //            _context.Add(userChat);
    //            await _context.SaveChangesAsync();
    //            return RedirectToAction(nameof(Index));
    //        }
    //        return View(userChat);
    //    }

    //    // GET: UserChats/Edit/5
    //    public async Task<IActionResult> Edit(string id)
    //    {
    //        if (id == null)
    //        {
    //            return NotFound();
    //        }

    //        var userChat = await _context.UserChats.FindAsync(id);
    //        if (userChat == null)
    //        {
    //            return NotFound();
    //        }
    //        return View(userChat);
    //    }

    //    // POST: UserChats/Edit/5
    //    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    //    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public async Task<IActionResult> Edit(string id, [Bind("UserID,UserName,GroupID")] UserChat userChat)
    //    {
    //        if (id != userChat.UserID)
    //        {
    //            return NotFound();
    //        }

    //        if (ModelState.IsValid)
    //        {
    //            try
    //            {
    //                _context.Update(userChat);
    //                await _context.SaveChangesAsync();
    //            }
    //            catch (DbUpdateConcurrencyException)
    //            {
    //                if (!UserChatExists(userChat.UserID))
    //                {
    //                    return NotFound();
    //                }
    //                else
    //                {
    //                    throw;
    //                }
    //            }
    //            return RedirectToAction(nameof(Index));
    //        }
    //        return View(userChat);
    //    }

    //    // GET: UserChats/Delete/5
    //    public async Task<IActionResult> Delete(string id)
    //    {
    //        if (id == null)
    //        {
    //            return NotFound();
    //        }

    //        var userChat = await _context.UserChats
    //            .FirstOrDefaultAsync(m => m.UserID == id);
    //        if (userChat == null)
    //        {
    //            return NotFound();
    //        }

    //        return View(userChat);
    //    }

    //    // POST: UserChats/Delete/5
    //    [HttpPost, ActionName("Delete")]
    //    [ValidateAntiForgeryToken]
    //    public async Task<IActionResult> DeleteConfirmed(string id)
    //    {
    //        var userChat = await _context.UserChats.FindAsync(id);
    //        _context.UserChats.Remove(userChat);
    //        await _context.SaveChangesAsync();
    //        return RedirectToAction(nameof(Index));
    //    }

    //    private bool UserChatExists(string id)
    //    {
    //        return _context.UserChats.Any(e => e.UserID == id);
    //    }


}