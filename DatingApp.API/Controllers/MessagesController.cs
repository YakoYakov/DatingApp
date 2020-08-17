using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers 
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository repo;
        private readonly IMapper mapper;
        public MessagesController(IDatingRepository repo, IMapper mapper)
        {
            this.mapper = mapper;
            this.repo = repo;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            Message message = await this.repo.GetMessageAsync(id);

            if (message == null)
            {
                return NotFound();
            }

            return Ok(message);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, CreateMessageViewModel messageModel)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            messageModel.SenderId = userId;

            User recipient = await this.repo.GetUserAsync(messageModel.RecipientId);

            if (recipient == null)
            {
                return BadRequest("Could not find user");
            }

            Message message = this.mapper.Map<Message>(messageModel);

            this.repo.Add(message);

            if (await this.repo.SaveAllAsync())
            {
                CreateMessageViewModel messageToReturn = this.mapper.Map<CreateMessageViewModel>(message);
                return CreatedAtRoute("GetMessage", new {userId, id = message.Id}, messageToReturn);
            }

            throw new Exception("Creating the message failed on save");
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(int userId, [FromQuery] MessageParams messageParams)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            messageParams.UserId = userId;

            PagedList<Message> messagesFromDb = await this.repo.GetMessagesForUserAsync(messageParams);

            IEnumerable<MessageToReturnViewModel> messages = this.mapper.Map<IEnumerable<MessageToReturnViewModel>>(messagesFromDb);

            Response.AddPagination(messagesFromDb.CurrentPage, messagesFromDb.PageSize, messagesFromDb.TotalCount, messagesFromDb.TotalPages);

            return Ok(messages);
        }
    }
}