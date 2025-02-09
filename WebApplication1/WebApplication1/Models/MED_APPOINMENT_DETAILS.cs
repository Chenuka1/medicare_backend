﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class MED_APPOINMENT_DETAILS
    {

        [Key]

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MAD_APPOINMENT_ID { get; set; }

        public string MAD_FULL_NAME { get; set; }

        public string MAD_CONTACT { get; set; }

        public int? MAD_PATIENT_NO { get; set; }

        public TimeSpan? MAD_START_TIME { get; set; }


        public TimeSpan? MAD_END_TIME { get; set; }


        public DateTime MAD_APPOINMENT_DATE { get; set; }


        public string? MAD_DOCTOR { get; set; }


        public string? MAD_EMAIL { get; set; }

        public TimeSpan? MAD_ALLOCATED_TIME { get; set; }

        [MaxLength(1)]
        public string? MAD_STATUS { get; set; }


        public string? MAD_PATIENT_CODE { get; set; }


        public int? MAD_SLOT_ID { get; set; }


        public string? MAD_USER_ID {  get; set; }   





    }
}
