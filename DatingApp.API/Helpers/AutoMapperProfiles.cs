using AutoMapper;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Creating maps for the classes we want the conversion for
            // We add configuration for the values which do not match the Model class
            // Or the values which require some calculation, like PhotoUrl or Age
            CreateMap<User, UserForListDto>()
                .ForMember(dest => dest.PhotoUrl, opt =>
                    opt.MapFrom(src =>
                        src.Photos.FirstOrDefault(p => p.IsMain).Url))

                .ForMember(dest => dest.Age, opt =>
                    opt.MapFrom(scr =>
                        scr.DateOfBirth.CalculateAge()));

            CreateMap<User, UserForDetailDto>()
                .ForMember(dest => dest.PhotoUrl, opt =>
                    opt.MapFrom(src =>
                        src.Photos.FirstOrDefault(p => p.IsMain).Url))

                .ForMember(dest => dest.Age, opt =>
                    opt.MapFrom(scr =>
                        scr.DateOfBirth.CalculateAge()));

            CreateMap<Photo, PhotosForDetailDto>();
            CreateMap<UserForUpdateDto, User>();
        }      

    }
}
