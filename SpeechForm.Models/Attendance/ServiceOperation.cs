namespace SpeechForm.Models.Attendance
{
    public class ServiceOperation
    {
        public ServiceOperation()
        {
            
        }

        public string ServiceOperationId { get; set; }
        public string ServiceOperationDescription { get; set; }
        
        public string SupervisorId { get; set; }
        public string SupervisorName { get; set; }
        public string SupervisorUsername { get; set; }

        public string AttendantId { get; set; }
        public string AttendantName { get; set; }
        public string AttendantUsername { get; set; }
    }
}
