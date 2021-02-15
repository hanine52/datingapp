using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Entities
{
    public class UserRepository : IUserRepository
    {
        // because this class can be accessing to the DBContext 
        // we ganna need a constructor
        // for injecting dataContext
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;

        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {// projection
            return await _context.Users
                .Where(x => x.UserName == username)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users.AsQueryable();
            query = query.Where(u => u.UserName != userParams.CurrentUsername);
            query = query.Where(u => u.Gender == userParams.Gender);

            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

            query = userParams.OrderBy switch 
            {
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive),
            };

            query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

            return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .AsNoTracking(), userParams.PageNumber, userParams.PageSize);
        }

        async Task<AppUser> IUserRepository.GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        async Task<AppUser> IUserRepository.GetUserByUsernameAsync(string username)
        {
            return await _context.Users.Include(p => p.Photos).SingleOrDefaultAsync(x => x.UserName == username);
        }

        async Task<IEnumerable<AppUser>> IUserRepository.GetUsersAsync()
        {
            return await _context.Users.Include(p => p.Photos).ToListAsync();
        }

        async Task<bool> IUserRepository.SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        void IUserRepository.Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}