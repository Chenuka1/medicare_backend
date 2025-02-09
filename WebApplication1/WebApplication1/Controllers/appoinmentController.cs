using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace webapplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<MED_APPOINMENT_DETAILS>> PostAppointment(MED_APPOINMENT_DETAILS appointment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.MED_APPOINMENT_DETAILS.Add(appointment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAppointmentById), new { id = appointment.MAD_APPOINMENT_ID }, appointment);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MED_APPOINMENT_DETAILS>> GetAppointmentById(int id)
        {
            var appointment = await _context.MED_APPOINMENT_DETAILS.FindAsync(id);

            if (appointment == null)
            {
                return NotFound();
            }

            return appointment;
        }


        [HttpGet("getappoinments/doctor")]
        public async Task<ActionResult<IEnumerable<object>>> GetAppointmentsByDoctorAndDate(string doctor, DateTime date)
        {
            var appointments = await (from appointment in _context.MED_APPOINMENT_DETAILS
                                      join timeslot in _context.MED_TIMESLOT
                                      on appointment.MAD_SLOT_ID equals timeslot.MT_SLOT_ID into ts
                                      from timeslot in ts.DefaultIfEmpty() // Left join to include null if no timeslot is found
                                      where appointment.MAD_DOCTOR == doctor && appointment.MAD_APPOINMENT_DATE.Date == date.Date
                                      select new
                                      {
                                          appointment.MAD_APPOINMENT_ID,
                                          appointment.MAD_FULL_NAME,
                                          appointment.MAD_CONTACT,
                                          appointment.MAD_PATIENT_NO,
                                          appointment.MAD_ALLOCATED_TIME,
                                          appointment.MAD_PATIENT_CODE,
                                          appointment.MAD_APPOINMENT_DATE,
                                          appointment.MAD_SLOT_ID, // Include slot ID from appointment
                                          TimeslotStartTime = timeslot != null ? timeslot.MT_START_TIME : (TimeSpan?)null, // Include timeslot details
                                          TimeslotEndTime = timeslot != null ? timeslot.MT_END_TIME : (TimeSpan?)null,
                                          TimeslotDate = timeslot != null ? timeslot.MT_SLOT_DATE : (DateTime?)null,
                                          IsCompleted = _context.MED_TREATMENT_DETAILS
                                              .Any(t => t.MTD_APPOINMENT_ID == appointment.MAD_APPOINMENT_ID) // Check if the appointment has treatment
                                      })
                                      .ToListAsync();

            if (appointments == null || !appointments.Any())
            {
                return NotFound("No appointments found for the selected date.");
            }

            return Ok(appointments);
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<MED_APPOINMENT_DETAILS>>> GetAllAppointments()
        {
            var appointments = await _context.MED_APPOINMENT_DETAILS
                .OrderBy(a => a.MAD_APPOINMENT_DATE)
                .ThenBy(a => a.MAD_PATIENT_NO)
                .ToListAsync();

            if (appointments == null || !appointments.Any())
            {
                return NotFound("No appointments found.");
            }

            return Ok(appointments);
        }


        // Get appointments by timeslot ID
        [HttpGet("appointments/{slotId}")]
        public async Task<IActionResult> GetAppointmentsByTimeslot(int slotId)
        {
            var appointments = await _context.MED_APPOINMENT_DETAILS
                .Where(a => a.MAD_SLOT_ID == slotId)
                .Select(a => new
                {
                    a.MAD_APPOINMENT_ID,
                    a.MAD_FULL_NAME,
                    a.MAD_CONTACT,
                    a.MAD_PATIENT_NO,
                    a.MAD_ALLOCATED_TIME,
                    a.MAD_PATIENT_CODE,
                    a.MAD_APPOINMENT_DATE,
                    IsCompleted = _context.MED_TREATMENT_DETAILS
                        .Any(t => t.MTD_APPOINMENT_ID == a.MAD_APPOINMENT_ID)
                })
                .ToListAsync();

            if (appointments == null || appointments.Count == 0)
            {
                return NotFound("No appointments found for the selected timeslot.");
            }

            return Ok(appointments);
        }

        [HttpGet("Test/{id}")]
        public async Task <IActionResult> Fetchbyid(int id)
        {


            var appoinments =await  _context.MED_APPOINMENT_DETAILS
                .Where(a => a.MAD_APPOINMENT_ID == id)
                .FirstOrDefaultAsync();

            if (appoinments == null)
            {
                return NotFound("No appointments found for the selected timeslot.");
            }


            return Ok(appoinments);
        }
       










        // Updated endpoint to match the correct route
        /*[HttpGet("getappointment/email")] // Corrected route
        public async Task<ActionResult<IEnumerable<MED_APPOINMENT_DETAILS>>> getappointmentemail(string email)
        {
            var appointments = await _context.MED_APPOINMENT_DETAILS
                .Where(a => a.MAD_EMAIL == email)
                .ToListAsync();

            if (appointments == null || !appointments.Any())
            {
                return NotFound("No appointments found");
            }

            return Ok(appointments);
        }*/






        /*[HttpGet("getappointment/patientid")]
        public async Task<ActionResult<IEnumerable>> getappoinmentbyid(string patientid)
        {
            var appointments=await _context.MED_APPOINMENT_DETAILS
                               .Where(a => a.MAD_PATIENT_CODE == patientid)
                               .Select(a => new
                               {

                                   a.MAD_APPOINMENT_ID,
                                   a.MAD_DOCTOR,
                                   a.MAD_APPOINMENT_DATE,
                                   a.MAD_START_TIME,
                                   a.MAD_END_TIME,
                                   a.MAD_ALLOCATED_TIME,
                                   TreatmentStatus = _context.MED_TREATMENT_DETAILS.Any(t => t.MTD_APPOINMENT_ID == a.MAD_APPOINMENT_ID) ? "Completed" : "Pending"




                               }
                               
                               
                               
                               
                               
                               
                               )






        }*/


        [HttpGet("getappointment/patientcode")]
        public async Task<ActionResult<IEnumerable<object>>> DetailsById(string patientcode)
        {
            if (string.IsNullOrWhiteSpace(patientcode))
            {
                return BadRequest("Patient code cannot be null or empty.");
            }

            var appointments = await _context.MED_APPOINMENT_DETAILS
                               .Where(a => a.MAD_PATIENT_CODE == patientcode)
                               .Select(a => new
                               {
                                   a.MAD_APPOINMENT_ID,
                                   a.MAD_DOCTOR,
                                   a.MAD_APPOINMENT_DATE,
                                   a.MAD_START_TIME,
                                   a.MAD_END_TIME,
                                   a.MAD_ALLOCATED_TIME,
                                   TreatmentStatus = _context.MED_TREATMENT_DETAILS.Any(t => t.MTD_APPOINMENT_ID == a.MAD_APPOINMENT_ID) ? "Completed" : "Pending"
                               })
                               .ToListAsync();

            if (appointments == null || !appointments.Any())
            {
                return NotFound("No appointments found for the given patient code.");
            }

            return Ok(appointments);
        }













        [HttpGet("getappointment/email")]
        public async Task<ActionResult<IEnumerable<object>>> getappointmentemail(string email)
        {
            var appointments = await _context.MED_APPOINMENT_DETAILS
                .Where(a => a.MAD_EMAIL == email)
                .Select(a => new
                {
                    a.MAD_APPOINMENT_ID,
                    a.MAD_DOCTOR,
                    a.MAD_APPOINMENT_DATE,
                    a.MAD_START_TIME,
                    a.MAD_END_TIME,
                    a.MAD_ALLOCATED_TIME,
                    TreatmentStatus = _context.MED_TREATMENT_DETAILS.Any(t => t.MTD_APPOINMENT_ID == a.MAD_APPOINMENT_ID) ? "Completed" : "Pending"
                })
                .ToListAsync();

            if (appointments == null || !appointments.Any())
            {
                return NotFound("No appointments found");
            }

            return Ok(appointments);
        }

    }
}
