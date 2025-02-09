﻿/*
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class CronJobService : BackgroundService
    {
        private readonly ILogger<CronJobService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CronJobService(ILogger<CronJobService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CronJobService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        var lastTimeSlot = await dbContext.MED_TIMESLOT
                            .OrderByDescending(ts => ts.MT_SLOT_DATE)
                            .FirstOrDefaultAsync(stoppingToken);

                        if (lastTimeSlot != null)
                        {
                            var newTimeSlot = new MED_TIMESLOT
                            {
                                MT_SLOT_DATE = lastTimeSlot.MT_SLOT_DATE.AddDays(1),
                                MT_START_TIME = lastTimeSlot.MT_START_TIME,
                                MT_END_TIME = lastTimeSlot.MT_END_TIME,
                                MT_PATIENT_NO = 0,
                                MT_MAXIMUM_PATIENTS = lastTimeSlot.MT_MAXIMUM_PATIENTS,
                                MT_DOCTOR = lastTimeSlot.MT_DOCTOR,
                                MT_ALLOCATED_TIME = lastTimeSlot.MT_START_TIME,
                                MT_USER_ID = lastTimeSlot.MT_USER_ID
                            };

                            dbContext.MED_TIMESLOT.Add(newTimeSlot);
                            await dbContext.SaveChangesAsync(stoppingToken);

                            _logger.LogInformation($"New time slot created for {newTimeSlot.MT_SLOT_DATE}.");
                        }
                        else
                        {
                            _logger.LogWarning("No time slots found to create a new one.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing cron job.");
                }

                // Run every 5 minutes
                *//*await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);*//*


                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
*/



using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class CronJobService : BackgroundService
    {
        private readonly ILogger<CronJobService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CronJobService(ILogger<CronJobService> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CronJobService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        // Get distinct doctors who have time slots
                        var distinctDoctors = await dbContext.MED_TIMESLOT
                            .Select(ts => ts.MT_DOCTOR)
                            .Where(d => d != null)
                            .Distinct()
                            .ToListAsync(stoppingToken);

                        foreach (var doctor in distinctDoctors)
                        {
                            // Get the latest time slot for each doctor
                            var lastTimeSlot = await dbContext.MED_TIMESLOT
                                .Where(ts => ts.MT_DOCTOR == doctor)
                                .OrderByDescending(ts => ts.MT_SLOT_DATE)
                                .FirstOrDefaultAsync(stoppingToken);

                            if (lastTimeSlot != null)
                            {
                                var newTimeSlot = new MED_TIMESLOT
                                {
                                    MT_SLOT_DATE = lastTimeSlot.MT_SLOT_DATE.AddDays(1), // Next day slot
                                    MT_START_TIME = lastTimeSlot.MT_START_TIME,
                                    MT_END_TIME = lastTimeSlot.MT_END_TIME,
                                    MT_PATIENT_NO = 0, // Reset patient count
                                    MT_MAXIMUM_PATIENTS = lastTimeSlot.MT_MAXIMUM_PATIENTS,
                                    MT_DOCTOR = lastTimeSlot.MT_DOCTOR,
                                    MT_ALLOCATED_TIME = lastTimeSlot.MT_START_TIME, // Reset allocated time
                                    MT_USER_ID = lastTimeSlot.MT_USER_ID
                                };

                                dbContext.MED_TIMESLOT.Add(newTimeSlot);
                                await dbContext.SaveChangesAsync(stoppingToken);

                                _logger.LogInformation($"New time slot created for {newTimeSlot.MT_DOCTOR} on {newTimeSlot.MT_SLOT_DATE}, with MT_ALLOCATED_TIME reset.");
                            }
                            else
                            {
                                _logger.LogWarning($"No time slot found for doctor {doctor} to create a new one.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing cron job.");
                }

                // Run every day at midnight
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);

                /*    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);*/

                /*  await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);*/
            }
        }
    }
}
