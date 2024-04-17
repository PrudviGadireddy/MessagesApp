using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MessagesApp.Context;
using MessagesApp.Models;
using HL7.Dotnetcore;
using System.IO;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Model;
using RabbitMQ.Client;
using System.Text;

namespace MessagesApp.Controllers
{
    public class myMessagesController : Controller
    {
        private readonly appDbContext _context;
        private string inputMsg;

        public myMessagesController(appDbContext context)
        {
            _context = context;
        }

        // GET: myMessages
        public async Task<IActionResult> Index()
        {
            return View(await _context.Messages.ToListAsync());
        }

        // PUBLISH: myMessages/SendToQueue/5
        public async Task<IActionResult> SendToQueue(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myMessage = await _context.Messages
                .FirstOrDefaultAsync(m => m.ID == id);

           
            var factory = new ConnectionFactory
            {

                HostName = "fly-01.rmq.cloudamqp.com",    
                Port = 5672,               
                UserName = "wmnzpswl",        
                Password = "Znw_tavDMUMZB0c81pXWavu8qBdiOZb0",
                VirtualHost = "wmnzpswl"
            };

            // Create a connection to the RabbitMQ server
            using var connection = factory.CreateConnection();

            // Create a channel
            using var channel = connection.CreateModel();

            // Declare a queue
            string queueName = "HL7_Messages";  // Replace with your queue name
            //channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            // Message to send
            byte[] body = Encoding.UTF8.GetBytes(myMessage.JSONmsg);

            var props = channel.CreateBasicProperties();
            props.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            props.Headers = new Dictionary<string, object>
            {
                {"Message ID", myMessage.ID}
            };

            // Publish the message to the queue
            channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: props, body: body);

            if (myMessage == null)
            {
                return NotFound();
            }

            return View(myMessage);
        }

        // GET: myMessages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myMessage = await _context.Messages
                .FirstOrDefaultAsync(m => m.ID == id);

            if (myMessage == null)
            {
                return NotFound();
            }

            return View(myMessage);
        }



        // GET: myMessages/Create
        public IActionResult Create()
        {
            return View();
        }


       
        // POST: myMessages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,msg,JSONmsg")] myMessage myMessage)
        {
            if (ModelState.IsValid)
            {
                //try {
                    //using (var sr = new StreamReader(@"./TestMessages/"+myMessage.JSONmsg))
                    //{
                    //    inputMsg = sr.ReadToEnd();
                    //}
                //}
                //catch { return View(); }
                
                var messageHL = new Message(myMessage.msg/*inputMsg*/);
                _ = messageHL.ParseMessage(true);
                myMessage.JSONmsg = messageHL.parsetoJson();
                _context.Add(myMessage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(myMessage);
        }

        // GET: myMessages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myMessage = await _context.Messages.FindAsync(id);
            if (myMessage == null)
            {
                return NotFound();
            }
            return View(myMessage);
        }

        // POST: myMessages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,msg,JSONmsg")] myMessage myMessage)
        {
            if (id != myMessage.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var messageHL = new Message(myMessage.msg);
                    _ = messageHL.ParseMessage(true);
                    myMessage.JSONmsg = messageHL.parsetoJson();
                    _context.Update(myMessage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!myMessageExists(myMessage.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(myMessage);
        }

        // GET: myMessages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var myMessage = await _context.Messages
                .FirstOrDefaultAsync(m => m.ID == id);
            if (myMessage == null)
            {
                return NotFound();
            }

            return View(myMessage);
        }

        // POST: myMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var myMessage = await _context.Messages.FindAsync(id);
            if (myMessage != null)
            {
                _context.Messages.Remove(myMessage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool myMessageExists(int id)
        {
            return _context.Messages.Any(e => e.ID == id);
        }
    }
}
