using System.ComponentModel.DataAnnotations;

namespace MyDockerApp.OptionsClasses
{
    public class RedisOptions
    {
        [Required] public string ConnectionString { get; set; } = string.Empty;
        [Required] public string InstanceName { get; set; } = string.Empty;
    }
}
