using AutoMapper;
using TaskTrackerApi.DTO.Projects;
using TaskTrackerApi.DTO.Tasks;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Mappings;

public class ProjectProfile : Profile
{
    public ProjectProfile()
    {
        CreateMap<Project, ProjectDto>();
        CreateMap<TaskItem, TaskItemDto>();
        CreateMap<Project, ProjectWithTasksDto>()
            .ForCtorParam("tasks", options => options.MapFrom(source => source.Tasks));
    }
}