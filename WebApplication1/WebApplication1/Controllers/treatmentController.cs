using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TreatmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TreatmentController> _logger;

        public TreatmentController(ApplicationDbContext context, ILogger<TreatmentController> logger)
        {
            _context = context;

            _logger = logger;
        }




        // GET: api/treatment/{patientId}/{serialNo}
        [HttpGet("{patientId}/{serialNo}")]
        public async Task<ActionResult<MED_TREATMENT_DETAILS>> GetById(string patientId, int serialNo)
        {
            var treatment = await _context.MED_TREATMENT_DETAILS
                                          .FirstOrDefaultAsync(t => t.MTD_PATIENT_CODE == patientId && t.MTD_SERIAL_NO == serialNo);

            if (treatment == null)
            {
                return NotFound();
            }

            return Ok(treatment);
        }


        [HttpPut("{patientID}/{serialNO}")]
        public async Task<ActionResult<MED_TREATMENT_DETAILS>> UpdateTreatmentDetails(string patientID, int serialNO, MED_TREATMENT_DETAILS updatedDetails)
        {
            // Fetch the existing treatment details based on patientID and serialNO
            var treatment = await _context.MED_TREATMENT_DETAILS
                                          .FirstOrDefaultAsync(t => t.MTD_PATIENT_CODE == patientID && t.MTD_SERIAL_NO == serialNO);

            if (treatment == null)
            {
                // If no treatment is found, return a NotFound response
                return NotFound(new { message = "Treatment details not found." });
            }

            // Update the fields with the new values from updatedDetails
            treatment.MTD_DATE = updatedDetails.MTD_DATE;
            treatment.MTD_DOCTOR = updatedDetails.MTD_DOCTOR;
            treatment.MTD_TYPE = updatedDetails.MTD_TYPE;
            treatment.MTD_COMPLAIN = updatedDetails.MTD_COMPLAIN;
            treatment.MTD_DIAGNOSTICS = updatedDetails.MTD_DIAGNOSTICS;
            treatment.MTD_REMARKS = updatedDetails.MTD_REMARKS;
            treatment.MTD_AMOUNT = updatedDetails.MTD_AMOUNT;
            treatment.MTD_PAYMENT_STATUS = updatedDetails.MTD_PAYMENT_STATUS;
            treatment.MTD_TREATMENT_STATUS = updatedDetails.MTD_TREATMENT_STATUS;
            treatment.MTD_SMS_STATUS = updatedDetails.MTD_SMS_STATUS;
            treatment.MTD_SMS = updatedDetails.MTD_SMS;
            treatment.MTD_MEDICAL_STATUS = updatedDetails.MTD_MEDICAL_STATUS;
            treatment.MTD_STATUS = updatedDetails.MTD_STATUS;
            treatment.MTD_UPDATED_BY = updatedDetails.MTD_UPDATED_BY;
            treatment.MTD_UPDATED_DATE = DateTime.Now;


            // Save the updated treatment details
            await _context.SaveChangesAsync();

            // Return the updated treatment details
            return Ok(treatment);
        }






        // POST: api/treatment
        [HttpPost]
        public async Task<ActionResult<MED_TREATMENT_DETAILS>> PostTreatment(MED_TREATMENT_DETAILS treatment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.MED_TREATMENT_DETAILS.Add(treatment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { patientId = treatment.MTD_PATIENT_CODE, serialNo = treatment.MTD_SERIAL_NO }, treatment);
        }



        // GET: api/treatment/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<MED_TREATMENT_DETAILS>> GetById(int id)
        {
            var treatment = await _context.MED_TREATMENT_DETAILS.FindAsync(id);

            if (treatment == null)
            {
                return NotFound();
            }

            return Ok(treatment);
        }

        // GET: api/treatment/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<IEnumerable<MED_TREATMENT_DETAILS>>> GetTreatmentsByPatientId(string patientId)
        {
            var treatments = await _context.MED_TREATMENT_DETAILS
                                           .Where(t => t.MTD_PATIENT_CODE == patientId)
                                           .ToListAsync();

            if (treatments == null || treatments.Count == 0)
            {
                return NotFound();
            }

            return Ok(treatments);
        }



        [HttpGet("match/{patientId}/{serialNo}")]
        public async Task<IActionResult> GetMatchedRecords(string patientId, int serialNo)
        {
            var result = await (
                                from d in _context.MED_DRUGS_DETAILS
                                join t in _context.MED_TREATMENT_DETAILS
                                on new { PatientCode = (string)d.MDD_PATIENT_CODE, SerialNo = (int)d.MDD_SERIAL_NO }
                                equals new { PatientCode = (string)t.MTD_PATIENT_CODE, SerialNo = (int)t.MTD_SERIAL_NO }
                                where t.MTD_PATIENT_CODE == patientId && t.MTD_SERIAL_NO == serialNo
                                select new
                                {
                                    t.MTD_PATIENT_CODE,
                                    t.MTD_SERIAL_NO,
                                    t.MTD_DATE,
                                    t.MTD_DOCTOR,
                                    t.MTD_TYPE,
                                    t.MTD_COMPLAIN,
                                    t.MTD_DIAGNOSTICS,
                                    t.MTD_REMARKS,
                                    t.MTD_AMOUNT,
                                    t.MTD_PAYMENT_STATUS,
                                    t.MTD_TREATMENT_STATUS,
                                    d.MDD_MATERIAL_CODE,
                                    d.MDD_QUANTITY,
                                    d.MDD_RATE,
                                    d.MDD_AMOUNT,
                                    d.MDD_DOSAGE,
                                    d.MDD_TAKES,
                                    d.MDD_GIVEN_QUANTITY,
                                    d.MDD_STATUS
                                }).ToListAsync();

            if (!result.Any())
            {
                return NotFound("No matching records found.");
            }

            return Ok(result);
        }


        [HttpGet("preparationcomplete/")]
        public async Task<IActionResult> GetPreparationCompleteDetails()
        {
            var result = await (from t in _context.MED_TREATMENT_DETAILS
                                join p in _context.MED_PATIENTS_DETAILS
                                on t.MTD_PATIENT_CODE equals p.MPD_PATIENT_CODE
                                where t.MTD_TREATMENT_STATUS == "P" // Filter for treatment status 'P'
                                select new
                                {
                                    // Patient details
                                    p.MPD_PATIENT_CODE,
                                    p.MPD_PATIENT_NAME,
                                    p.MPD_MOBILE_NO,
                                    p.MPD_NIC_NO
                                    /* p.MPD_CITY,
                                     p.MPD_ADDRESS,
                                     p.MPD_GUARDIAN,
                                     p.MPD_GUARDIAN_CONTACT_NO,
                                     p.MPD_birthdate*/,

                                    // Treatment details
                                    t.MTD_SERIAL_NO,
                                    t.MTD_DATE,
                                    t.MTD_DOCTOR,
                                    t.MTD_TYPE,
                                    t.MTD_COMPLAIN,
                                    t.MTD_DIAGNOSTICS,
                                    t.MTD_REMARKS,
                                    t.MTD_AMOUNT,
                                    t.MTD_PAYMENT_STATUS,
                                    t.MTD_TREATMENT_STATUS
                                }).ToListAsync();

            if (!result.Any())
            {
                return NotFound("No patients found with treatment preparation status 'P'.");
            }

            return Ok(result);
        }


        [HttpPatch("update/status/{patientId}/{serialNo}")]
        public async Task<IActionResult> UpdateTreatmentStatus(string patientId, int serialNo)
        {
            var treatment = await _context.MED_TREATMENT_DETAILS
                                          .FirstOrDefaultAsync(t => t.MTD_PATIENT_CODE == patientId && t.MTD_SERIAL_NO == serialNo);

            if (treatment == null)
            {
                return NotFound();
            }

            // Update the status to "C"
            treatment.MTD_TREATMENT_STATUS = "C";
            await _context.SaveChangesAsync();

            return NoContent(); // Return a 204 No Content response
        }

        [HttpGet("{serialNo}")]
        public async Task<IActionResult> GetTreatmentBySerialNumber(int serialNo)
        {
            var treatment = await _context.MED_TREATMENT_DETAILS
                           .Where(t => t.MTD_SERIAL_NO == serialNo)
                           .ToListAsync();

            if (treatment == null)
            {
                return NotFound();  // Return 404 if no treatment is found
            }

            return Ok(treatment);  // Return the treatment details if found
        }



        [HttpGet("patientdetail/treatmentdetail/{patientId}/{serialNo}")]
        public async Task<IActionResult> gettreatmentrecord(string patientId, int serialNo)
        {
            var treatmentquery = from t in _context.MED_TREATMENT_DETAILS
                                 where t.MTD_PATIENT_CODE == patientId && t.MTD_SERIAL_NO == serialNo
                                 select t;

            var drugsquery = from d in _context.MED_DRUGS_DETAILS
                             where d.MDD_PATIENT_CODE == patientId && d.MDD_SERIAL_NO == serialNo
                             join m in _context.MED_MATERIAL_CATALOGUE
                             on d.MDD_MATERIAL_CODE equals m.MMC_MATERIAL_CODE
                             select new
                             {
                                 d.MDD_MATERIAL_CODE,
                                 d.MDD_QUANTITY,
                                 d.MDD_RATE,
                                 d.MDD_AMOUNT,
                                 d.MDD_DOSAGE,
                                 d.MDD_TAKES,
                                 d.MDD_GIVEN_QUANTITY,
                                 d.MDD_STATUS,
                                 MDD_MATERIAL_NAME = m.MMC_DESCRIPTION // Changed property name
                             };

            var treatmentRecord = await (from t in treatmentquery
                                         select new
                                         {
                                             t.MTD_PATIENT_CODE,
                                             t.MTD_SERIAL_NO,
                                             t.MTD_DATE,
                                             t.MTD_DOCTOR,
                                             t.MTD_TYPE,
                                             t.MTD_COMPLAIN,
                                             t.MTD_DIAGNOSTICS,
                                             t.MTD_REMARKS,
                                             t.MTD_AMOUNT,
                                             t.MTD_TREATMENT_STATUS,
                                             Drugs = drugsquery.ToList()
                                         }).FirstOrDefaultAsync();

            if (treatmentRecord == null)
            {
                return NotFound("Treatment record not found.");
            }

            return Ok(treatmentRecord);
        }




        





















        [HttpGet("patient/record/{patientId}/{serialNo}")]
        public async Task<IActionResult> GetTreatmentRecord(string patientId, int serialNo)
        {


            var firstSerialNo = await _context.MED_TREATMENT_DETAILS
                .Where( t => t.MTD_PATIENT_CODE== patientId)// Inorder to find the treatment number I want to find the first treatment number
                .OrderBy(t => t.MTD_SERIAL_NO)
                .Select(t => t.MTD_SERIAL_NO)
                .FirstOrDefaultAsync();


            var treatmentQuery = from t in _context.MED_TREATMENT_DETAILS
                                 where t.MTD_PATIENT_CODE == patientId && t.MTD_SERIAL_NO == serialNo
                                 select t;

            var drugsQuery = from d in _context.MED_DRUGS_DETAILS
                             where d.MDD_PATIENT_CODE == patientId && d.MDD_SERIAL_NO == serialNo && d.MDD_STATUS != "I" // The condition is I used a patientid, serial number and status is not active
                             join m in _context.MED_MATERIAL_CATALOGUE
                             on d.MDD_MATERIAL_CODE equals m.MMC_MATERIAL_CODE
                             select new
                             {
                                 d.MDD_MATERIAL_CODE,
                                 d.MDD_QUANTITY,
                                 d.MDD_RATE,
                                 d.MDD_AMOUNT,
                                 d.MDD_DOSAGE,
                                 d.MDD_TAKES,
                                 d.MDD_GIVEN_QUANTITY,
                                 d.MDD_STATUS,
                               
                                 DrugName = m.MMC_DESCRIPTION


                             };

            var treatmentRecord = await (from t in treatmentQuery
                                         select new
                                         {
                                             t.MTD_PATIENT_CODE,
                                             t.MTD_SERIAL_NO,
                                             t.MTD_DATE,
                                             t.MTD_DOCTOR,
                                             t.MTD_TYPE,
                                             t.MTD_COMPLAIN,
                                             t.MTD_DIAGNOSTICS,
                                             t.MTD_REMARKS,
                                             t.MTD_AMOUNT,
                                             Treatmentnumber =  (t.MTD_SERIAL_NO - firstSerialNo + 1),
                                             t.MTD_TREATMENT_STATUS,
                                             Drugs = drugsQuery.ToList()
                                         }).FirstOrDefaultAsync();

            if (treatmentRecord == null)
            {
                return NotFound("Treatment record not found.");
            }

            return Ok(treatmentRecord);
        }



        [HttpGet("Gettreatments/{patientId}")]
        public async Task<IActionResult> GetTreatmentAmount(string patientId)
        {
            // Validate input
            if (string.IsNullOrEmpty(patientId))
            {
                return BadRequest(new { Message = "Patient code is required" });
            }

            try
            {
                // Use CountAsync for asynchronous operation
                var treatmentCount = await _context.MED_TREATMENT_DETAILS
                    .Where(t => t.MTD_PATIENT_CODE == patientId)
                    .CountAsync();

                // Return the response with a treatment count of zero if no treatments are found
                return Ok(new
                {
                    PatientCode = patientId,
                    TreatmentCount = treatmentCount
                });
            }
            catch (Exception ex)
            {
                // Log the error (optional, recommended)
                return StatusCode(500, new
                {
                    Message = "An error occurred while fetching treatment details.",
                    Error = ex.Message
                });
            }
        }



        /* [HttpPut("patientdetail/treatmentdetail/{patientId}/{serialNo}")]
         public async Task<IActionResult> updateTreatmentRecord(string patientId, int serialNo, [FromBody] TreatmentUpdateModel treatmentUpdate)
         {
             // Retrieve the treatment record to update
             var treatment = await _context.MED_TREATMENT_DETAILS
                                         .FirstOrDefaultAsync(t => t.MTD_PATIENT_CODE == patientId && t.MTD_SERIAL_NO == serialNo);

             if (treatment == null)
             {
                 return NotFound("Treatment record not found.");
             }

             // Update the treatment details
             treatment.MTD_DATE = treatmentUpdate.MTD_DATE;
             treatment.MTD_DOCTOR = treatmentUpdate.MTD_DOCTOR;
             treatment.MTD_TYPE = treatmentUpdate.MTD_TYPE;
             treatment.MTD_COMPLAIN = treatmentUpdate.MTD_COMPLAIN;
             treatment.MTD_DIAGNOSTICS = treatmentUpdate.MTD_DIAGNOSTICS;
             treatment.MTD_REMARKS = treatmentUpdate.MTD_REMARKS;
             treatment.MTD_AMOUNT = treatmentUpdate.MTD_AMOUNT;
             treatment.MTD_TREATMENT_STATUS = treatmentUpdate.MTD_TREATMENT_STATUS;

             // Update the drug records
             foreach (var drugUpdate in treatmentUpdate.Drugs)
             {
                 var drug = await _context.MED_DRUGS_DETAILS
                                         .FirstOrDefaultAsync(d => d.MDD_PATIENT_CODE == patientId && d.MDD_SERIAL_NO == serialNo && d.MDD_MATERIAL_CODE == drugUpdate.MDD_MATERIAL_CODE);

                 if (drug != null)
                 {
                     drug.MDD_QUANTITY = drugUpdate.MDD_QUANTITY;
                     drug.MDD_RATE = drugUpdate.MDD_RATE;
                     drug.MDD_AMOUNT = drugUpdate.MDD_AMOUNT;
                     drug.MDD_DOSAGE = drugUpdate.MDD_DOSAGE;
                     drug.MDD_TAKES = drugUpdate.MDD_TAKES;
                     drug.MDD_GIVEN_QUANTITY = drugUpdate.MDD_GIVEN_QUANTITY;
                     drug.MDD_STATUS = drugUpdate.MDD_STATUS;
                 }
                 else
                 {
                     // If drug record doesn't exist, create a new one (optional)
                     _context.MED_DRUGS_DETAILS.Add(new MED_DRUGS_DETAILS
                     {
                         MDD_PATIENT_CODE = patientId,
                         MDD_SERIAL_NO = serialNo,
                         MDD_MATERIAL_CODE = drugUpdate.MDD_MATERIAL_CODE,
                         MDD_QUANTITY = drugUpdate.MDD_QUANTITY,
                         MDD_RATE = drugUpdate.MDD_RATE,
                         MDD_AMOUNT = drugUpdate.MDD_AMOUNT,
                         MDD_DOSAGE = drugUpdate.MDD_DOSAGE,
                         MDD_TAKES = drugUpdate.MDD_TAKES,
                         MDD_GIVEN_QUANTITY = drugUpdate.MDD_GIVEN_QUANTITY,
                         MDD_STATUS = drugUpdate.MDD_STATUS
                     });
                 }
             }

             // Save the changes to the database
             await _context.SaveChangesAsync();

             return Ok("Treatment and drug records updated successfully.");
         }
 */














        /* [HttpPut("patientdetail/treatmentdrugs/{patientId}/{serialNo}")]
         public async Task<IActionResult> UpdateTreatmentAndDrugsDetails(
      string patientId,
      int serialNo,
      [FromBody] TreatmentAndDrugsUpdateModel updateModel)
         {
             // Begin a transaction
             using var transaction = await _context.Database.BeginTransactionAsync();

             try
             {
                 // Fetch the treatment record
                 var treatment = await _context.MED_TREATMENT_DETAILS
                     .FirstOrDefaultAsync(t => t.MTD_PATIENT_CODE == patientId && t.MTD_SERIAL_NO == serialNo);

                 if (treatment == null)
                 {
                     return NotFound("Treatment record not found.");
                 }

                 // Update treatment record
                 treatment.MTD_DOCTOR = updateModel.Treatment.MTD_DOCTOR;
                 treatment.MTD_TYPE = updateModel.Treatment.MTD_TYPE;
                 treatment.MTD_COMPLAIN = updateModel.Treatment.MTD_COMPLAIN;
                 treatment.MTD_DIAGNOSTICS = updateModel.Treatment.MTD_DIAGNOSTICS;
                 treatment.MTD_REMARKS = updateModel.Treatment.MTD_REMARKS;
                 treatment.MTD_AMOUNT = updateModel.Treatment.MTD_AMOUNT;
                 treatment.MTD_TREATMENT_STATUS = updateModel.Treatment.MTD_TREATMENT_STATUS;

                 _context.MED_TREATMENT_DETAILS.Update(treatment);

                 // Update/Add drug records
                 foreach (var drugUpdate in updateModel.Drugs)
                 {
                     // Check if the drug exists
                     var drug = await _context.MED_DRUGS_DETAILS
                         .FirstOrDefaultAsync(d =>
                             d.MDD_PATIENT_CODE == patientId &&
                             d.MDD_SERIAL_NO == serialNo &&
                             d.MDD_MATERIAL_CODE == drugUpdate.MDD_MATERIAL_CODE);

                     if (drug != null)
                     {
                         // Update existing drug
                         drug.MDD_MATERIAL_CODE = drugUpdate.MDD_MATERIAL_CODE;
                         drug.MDD_QUANTITY = drugUpdate.MDD_QUANTITY;
                         drug.MDD_RATE = drugUpdate.MDD_RATE;
                         drug.MDD_AMOUNT = drugUpdate.MDD_AMOUNT;
                         drug.MDD_DOSAGE = drugUpdate.MDD_DOSAGE;
                         drug.MDD_TAKES = drugUpdate.MDD_TAKES;
                         drug.MDD_GIVEN_QUANTITY = drugUpdate.MDD_GIVEN_QUANTITY;
                         drug.MDD_STATUS = drugUpdate.MDD_STATUS;

                         _context.MED_DRUGS_DETAILS.Update(drug);
                     }
                     else
                     {
                         // Add new drug if not exists
                         var newDrug = new MED_DRUGS_DETAILS
                         {
                             MDD_PATIENT_CODE = patientId,
                             MDD_SERIAL_NO = serialNo,
                             MDD_MATERIAL_CODE = drugUpdate.MDD_MATERIAL_CODE,
                             MDD_QUANTITY = drugUpdate.MDD_QUANTITY,
                             MDD_RATE = drugUpdate.MDD_RATE,
                             MDD_AMOUNT = drugUpdate.MDD_AMOUNT,
                             MDD_DOSAGE = drugUpdate.MDD_DOSAGE,
                             MDD_TAKES = drugUpdate.MDD_TAKES,
                             MDD_GIVEN_QUANTITY = drugUpdate.MDD_GIVEN_QUANTITY,
                             MDD_STATUS = drugUpdate.MDD_STATUS
                         };

                         await _context.MED_DRUGS_DETAILS.AddAsync(newDrug);
                     }
                 }

                 // Save changes to the database
                 await _context.SaveChangesAsync();

                 // Commit the transaction
                 await transaction.CommitAsync();

                 return Ok("Treatment and drug records updated successfully.");
             }
             catch (Exception ex)
             {
                 // Rollback the transaction in case of an error
                 await transaction.RollbackAsync();
                 return StatusCode(500, $"An error occurred: {ex.Message}");
             }
         }
 */


        [HttpPost("updatingtreatment/{patientid}/{serialno}")]
        public async Task<IActionResult> UpdateTreatmentAndPrescriptions(string patientid, int serialno, [FromBody] TreatmentAndDrugsUpdateModel updateModel)
        {
            // Fetch the Treatment record
            var treatment = await _context.MED_TREATMENT_DETAILS
                .FirstOrDefaultAsync(t => t.MTD_PATIENT_CODE == patientid && t.MTD_SERIAL_NO == serialno);

            if (treatment == null)
            {
                return NotFound("Treatment not found.");
            }

            // Update treatment fields
            treatment.MTD_COMPLAIN = updateModel.Treatment.MTD_COMPLAIN;
            treatment.MTD_DIAGNOSTICS = updateModel.Treatment.MTD_DIAGNOSTICS;
            treatment.MTD_REMARKS = updateModel.Treatment.MTD_REMARKS;
            treatment.MTD_AMOUNT = updateModel.Treatment.MTD_AMOUNT;
            treatment.MTD_TREATMENT_STATUS = updateModel.Treatment.MTD_TREATMENT_STATUS;

            // Save treatment updates
            _context.MED_TREATMENT_DETAILS.Update(treatment);

            // Fetch existing prescriptions for the treatment
            var existingPrescriptions = await _context.MED_DRUGS_DETAILS
                .Where(p => p.MDD_PATIENT_CODE == patientid && p.MDD_SERIAL_NO == serialno)
                .ToListAsync();

            foreach (var drug in updateModel.Drugs)
            {
                // Check if the prescription exists
                var existingPrescription = existingPrescriptions
                    .FirstOrDefault(p => p.MDD_MATERIAL_CODE == drug.MDD_MATERIAL_CODE);

                if (existingPrescription != null)
                {
                    // Update the existing prescription
                    existingPrescription.MDD_QUANTITY = drug.MDD_QUANTITY;
                    existingPrescription.MDD_RATE = drug.MDD_RATE;
                    existingPrescription.MDD_AMOUNT = drug.MDD_AMOUNT;
                    existingPrescription.MDD_DOSAGE = drug.MDD_DOSAGE;
                    existingPrescription.MDD_TAKES = drug.MDD_TAKES;
                    existingPrescription.MDD_GIVEN_QUANTITY = drug.MDD_GIVEN_QUANTITY;
                    existingPrescription.MDD_STATUS = drug.MDD_STATUS;
                }
                else
                {
                    // Add a new prescription if it doesn't exist
                    var newPrescription = new MED_DRUGS_DETAILS
                    {
                        MDD_PATIENT_CODE = patientid,
                        MDD_SERIAL_NO = serialno,
                        MDD_MATERIAL_CODE = drug.MDD_MATERIAL_CODE,
                        MDD_QUANTITY = drug.MDD_QUANTITY,
                        MDD_RATE = drug.MDD_RATE,
                       /* MDD_AMOUNT = drug.MDD_AMOUNT,*/
                       MDD_AMOUNT= drug.MDD_RATE* drug.MDD_QUANTITY,
                        MDD_DOSAGE = drug.MDD_DOSAGE,
                        MDD_TAKES = drug.MDD_TAKES,
                        MDD_GIVEN_QUANTITY = drug.MDD_GIVEN_QUANTITY,
                        MDD_STATUS = drug.MDD_STATUS
                      
                    };
                    await _context.MED_DRUGS_DETAILS.AddAsync(newPrescription);
                }
            }

            // Save all changes to the database
            await _context.SaveChangesAsync();

            return Ok("Treatment and prescriptions updated successfully.");
        }



    }






    /*[HttpPost("patientdetail/treatmentdrugs/{patientId}/{serialNo}")]
        public async Task<IActionResult> UpdateTreatmentAndDrugsDetails(string patientId, int serialNo, [FromBody] TreatmentAndDrugsUpdateModel updateModel)
        {
            if (string.IsNullOrWhiteSpace(patientId))
            {
                return BadRequest("Patient ID is required.");
            }

            if (updateModel == null)
            {
                return BadRequest("Request body cannot be null.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Fetch the treatment record
                var treatment = await _context.MED_TREATMENT_DETAILS
                    .FirstOrDefaultAsync(t => t.MTD_PATIENT_CODE == patientId && t.MTD_SERIAL_NO == serialNo);

                if (treatment == null)
                {
                    return NotFound($"No treatment record found for patient ID '{patientId}' and serial number '{serialNo}'.");
                }

                // Validate treatment data
                if (updateModel.Treatment == null)
                {
                    return BadRequest("Treatment data is required for updating.");
                }

                // Update treatment record
                treatment.MTD_DOCTOR = updateModel.Treatment.MTD_DOCTOR;
                treatment.MTD_TYPE = updateModel.Treatment.MTD_TYPE;
                treatment.MTD_COMPLAIN = updateModel.Treatment.MTD_COMPLAIN;
                treatment.MTD_DIAGNOSTICS = updateModel.Treatment.MTD_DIAGNOSTICS;
                treatment.MTD_REMARKS = updateModel.Treatment.MTD_REMARKS;
                treatment.MTD_AMOUNT = updateModel.Treatment.MTD_AMOUNT;
                treatment.MTD_TREATMENT_STATUS = updateModel.Treatment.MTD_TREATMENT_STATUS;

                _context.MED_TREATMENT_DETAILS.Update(treatment);

                // Update/Add drug records
                if (updateModel.Drugs != null && updateModel.Drugs.Any())
                {
                    foreach (var drugUpdate in updateModel.Drugs)
                    {
                        if (string.IsNullOrWhiteSpace(drugUpdate.MDD_MATERIAL_CODE))
                        {
                            return BadRequest("Material code is required for drug updates.");
                        }

                        // Validate if the material code exists
                        var materialExists = await _context.MED_MATERIAL_CATALOGUE
                            .AnyAsync(m => m.MMC_MATERIAL_CODE == drugUpdate.MDD_MATERIAL_CODE);

                        if (!materialExists)
                        {
                            return BadRequest($"Invalid material code '{drugUpdate.MDD_MATERIAL_CODE}'. Please verify the material code.");
                        }

                        // Check if the drug exists in
                        var drug = await _context.MED_DRUGS_DETAILS
                            .FirstOrDefaultAsync(d =>
                                d.MDD_PATIENT_CODE == patientId &&
                                d.MDD_SERIAL_NO == serialNo &&
                                d.MDD_MATERIAL_CODE == drugUpdate.MDD_MATERIAL_CODE);

                        if (drug != null)
                        {
                            // Update existing drug
                            drug.MDD_QUANTITY = drugUpdate.MDD_QUANTITY;
                            drug.MDD_RATE = drugUpdate.MDD_RATE;
                            drug.MDD_AMOUNT = drugUpdate.MDD_AMOUNT;
                            drug.MDD_DOSAGE = drugUpdate.MDD_DOSAGE;
                            drug.MDD_TAKES = drugUpdate.MDD_TAKES;
                            drug.MDD_GIVEN_QUANTITY = drugUpdate.MDD_GIVEN_QUANTITY;
                            drug.MDD_STATUS = drugUpdate.MDD_STATUS;

                            _context.MED_DRUGS_DETAILS.Update(drug);

                            // Add new drug if not exists
                            var newDrug = new MED_DRUGS_DETAILS
                            {
                                MDD_PATIENT_CODE = patientId,
                                MDD_SERIAL_NO = serialNo,
                                MDD_MATERIAL_CODE = drugUpdate.MDD_MATERIAL_CODE,
                                MDD_QUANTITY = drugUpdate.MDD_QUANTITY,
                                MDD_RATE = drugUpdate.MDD_RATE,
                                MDD_AMOUNT = drugUpdate.MDD_AMOUNT,
                                MDD_DOSAGE = drugUpdate.MDD_DOSAGE,
                                MDD_TAKES = drugUpdate.MDD_TAKES,
                                MDD_GIVEN_QUANTITY = drugUpdate.MDD_GIVEN_QUANTITY,
                                MDD_STATUS = "I",//drugUpdate.MDD_STATUS
                            };
                            *//*  _logger.LogInformation($"Adding new drug: {JsonConvert.SerializeObject(newDrug)}");*//*


                            await _context.MED_DRUGS_DETAILS.AddAsync(newDrug);
                        }



                    }
                }
                else
                {
                    return BadRequest("Drug details are required for updating or adding new records.");
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Commit the transaction
                await transaction.CommitAsync();

                return Ok("Treatment and drug records updated successfully.");
            }
            catch (DbUpdateConcurrencyException dbConEx)
            {
                await transaction.RollbackAsync();
                // Log the exception
                // _logger.LogError(dbConEx, "Concurrency error while updating records.");
                return StatusCode(409, "A concurrency error occurred. Please try again.");
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                // Log database update exception
                // _logger.LogError(dbEx, "Database update error occurred.");
                return StatusCode(500, "A database error occurred while updating the records.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Log unexpected exceptions
                // _logger.LogError(ex, "Unexpected error occurred.");
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }
*/

       








        public class TreatmentAndDrugsUpdateModel
        {
            public TreatmentUpdateModel Treatment { get; set; }
            public List<DrugUpdateModel> Drugs { get; set; }
        }

        public class TreatmentUpdateModel
        {
            public string? MTD_DOCTOR { get; set; }
            public string? MTD_TYPE { get; set; }
            public string? MTD_COMPLAIN { get; set; }
            public string? MTD_DIAGNOSTICS { get; set; }
            public string? MTD_REMARKS { get; set; }
            public decimal MTD_AMOUNT { get; set; }
            public string? MTD_TREATMENT_STATUS { get; set; }


            
        }

        public class DrugUpdateModel
        {
            
            public string? MDD_MATERIAL_CODE { get; set; }
            public decimal MDD_QUANTITY { get; set; }
            public decimal? MDD_RATE { get; set; }
            public decimal? MDD_AMOUNT { get; set; }
            public string? MDD_DOSAGE { get; set; }
            public string? MDD_TAKES { get; set; }
            public decimal MDD_GIVEN_QUANTITY { get; set; }
            public string? MDD_STATUS { get; set; }
        }



    }
