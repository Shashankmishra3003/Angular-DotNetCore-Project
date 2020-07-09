using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;

        public DatingRepository(DataContext context)
        {
            _context = context;
        }

        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            // checks if the user has already liked the other user
            return await _context.Likes.FirstOrDefaultAsync(u => u.LikerId == userId
                                  && u.LikeeId == recipientId);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(u => u.UserId == userId)
                                        .FirstOrDefaultAsync(p => p.IsMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            //returning a normal list without pagination
            //var users = await _context.Users.Include(p => p.Photos).ToListAsync();
            //return users;
            
            // getting users from the database as Queryable 
            var users = _context.Users.Include(p => p.Photos)
                           .OrderByDescending(u => u.LastActive)
                           .AsQueryable();

            // filtering the user data using where clause
            users = users.Where(u => u.Id != userParams.UserId);

            users = users.Where(u => u.Gender == userParams.Gender);

            if(userParams.Likers)
            {
                // gets the list of users who like the current user
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);

                // filtering the list for curent user likes
                users = users.Where(u => userLikers.Contains(u.Id));
            }

            if(userParams.Likees)
            {
                // getting the users likes by current user
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikees.Contains(u.Id));
            }

            if(userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                // getitng the range of min and max ages
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge - 1);

                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }

            if(!string.IsNullOrEmpty(userParams.Orderby))
            {
                switch (userParams.Orderby)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }

            // creating a new instanc eof paged list using helper class
            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        // return the list of integers
        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _context.Users
                            .Include(u => u.Likers)
                            .Include(u => u.Likees)
                            .FirstOrDefaultAsync(u => u.Id == id);
            if (likers)
            {
                // returning the id list of likers of current User
                return user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
            }
            else
            {
                // returning the id list of user who are like by current user
                return user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);
            }
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;  //returns true when the value is > 0 else false
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<PagedList<Message>> GetMessageForUser(MessageParams messageParams)
        {
            // We need the photo with the message, ThenInclude()
            var messages = _context.Messages.Include(u => u.Sender)
                                   .ThenInclude(p => p.Photos)
                                   .Include(u => u.Recipient)
                                   .ThenInclude(p => p.Photos)
                                   .AsQueryable();

            // Condition for Inbox, Sent and Outbox and retuning the non deleted messages
            switch(messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId
                                           && u.RecipientDeleted == false);
                    break;
                case "Outbox":
                    messages = messages.Where(u => u.SenderId == messageParams.UserId
                                            && u.SenderDeleted == false);
                    break;
                default:
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId
                                            && u.RecipientDeleted == false
                                            && u.IsRead == false);
                    break;
            }

            messages = messages.OrderByDescending(d => d.MessageSend);
            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        // Getting only the non deleted message thread for sender and recipient
        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var messages =await _context.Messages.Include(u => u.Sender)
                                   .ThenInclude(p => p.Photos)
                                   .Include(u => u.Recipient)
                                   .ThenInclude(p => p.Photos)
                                   .Where(u => u.RecipientId == userId && u.RecipientDeleted == false
                                          && u.SenderId == recipientId && u.SenderDeleted == false
                                          || u.RecipientId == recipientId && u.SenderId == userId)
                                   .OrderByDescending(m => m.MessageSend)
                                   .ToListAsync();
            return messages;
                                   
        }
    }
}
