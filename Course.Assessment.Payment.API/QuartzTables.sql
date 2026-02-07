CREATE SCHEMA IF NOT EXISTS quartz;
SET search_path TO quartz;

CREATE TABLE qrtz_job_details (
  sched_name TEXT NOT NULL,
  job_name TEXT NOT NULL,
  job_group TEXT NOT NULL,
  description TEXT,
  job_class_name TEXT NOT NULL,
  is_durable BOOLEAN NOT NULL,
  is_nonconcurrent BOOLEAN NOT NULL,
  is_update_data BOOLEAN NOT NULL,
  requests_recovery BOOLEAN NOT NULL,
  job_data BYTEA,
  PRIMARY KEY (sched_name, job_name, job_group)
);

CREATE TABLE qrtz_triggers (
  sched_name TEXT NOT NULL,
  trigger_name TEXT NOT NULL,
  trigger_group TEXT NOT NULL,
  job_name TEXT NOT NULL,
  job_group TEXT NOT NULL,
  description TEXT,
  next_fire_time BIGINT,
  prev_fire_time BIGINT,
  priority INTEGER,
  trigger_state TEXT NOT NULL,
  trigger_type TEXT NOT NULL,
  start_time BIGINT NOT NULL,
  end_time BIGINT,
  calendar_name TEXT,
  misfire_instr SMALLINT,
  job_data BYTEA,
  PRIMARY KEY (sched_name, trigger_name, trigger_group),
  FOREIGN KEY (sched_name, job_name, job_group)
    REFERENCES qrtz_job_details
);

CREATE TABLE qrtz_simple_triggers (
  sched_name TEXT NOT NULL,
  trigger_name TEXT NOT NULL,
  trigger_group TEXT NOT NULL,
  repeat_count BIGINT NOT NULL,
  repeat_interval BIGINT NOT NULL,
  times_triggered BIGINT NOT NULL,
  PRIMARY KEY (sched_name, trigger_name, trigger_group),
  FOREIGN KEY (sched_name, trigger_name, trigger_group)
    REFERENCES qrtz_triggers
    ON DELETE CASCADE
);

CREATE TABLE qrtz_cron_triggers (
  sched_name TEXT NOT NULL,
  trigger_name TEXT NOT NULL,
  trigger_group TEXT NOT NULL,
  cron_expression TEXT NOT NULL,
  time_zone_id TEXT,
  PRIMARY KEY (sched_name, trigger_name, trigger_group),
  FOREIGN KEY (sched_name, trigger_name, trigger_group)
    REFERENCES qrtz_triggers
    ON DELETE CASCADE
);

CREATE TABLE qrtz_simprop_triggers (
  sched_name TEXT NOT NULL,
  trigger_name TEXT NOT NULL,
  trigger_group TEXT NOT NULL,
  str_prop_1 TEXT,
  str_prop_2 TEXT,
  str_prop_3 TEXT,
  int_prop_1 INTEGER,
  int_prop_2 INTEGER,
  long_prop_1 BIGINT,
  long_prop_2 BIGINT,
  dec_prop_1 NUMERIC(13,4),
  dec_prop_2 NUMERIC(13,4),
  bool_prop_1 BOOLEAN,
  bool_prop_2 BOOLEAN,
  PRIMARY KEY (sched_name, trigger_name, trigger_group),
  FOREIGN KEY (sched_name, trigger_name, trigger_group)
    REFERENCES qrtz_triggers
    ON DELETE CASCADE
);

CREATE TABLE qrtz_blob_triggers (
  sched_name TEXT NOT NULL,
  trigger_name TEXT NOT NULL,
  trigger_group TEXT NOT NULL,
  blob_data BYTEA,
  PRIMARY KEY (sched_name, trigger_name, trigger_group),
  FOREIGN KEY (sched_name, trigger_name, trigger_group)
    REFERENCES qrtz_triggers
    ON DELETE CASCADE
);

CREATE TABLE qrtz_calendars (
  sched_name TEXT NOT NULL,
  calendar_name TEXT NOT NULL,
  calendar BYTEA NOT NULL,
  PRIMARY KEY (sched_name, calendar_name)
);

CREATE TABLE qrtz_paused_trigger_grps (
  sched_name TEXT NOT NULL,
  trigger_group TEXT NOT NULL,
  PRIMARY KEY (sched_name, trigger_group)
);

CREATE TABLE qrtz_fired_triggers (
  sched_name TEXT NOT NULL,
  entry_id TEXT NOT NULL,
  trigger_name TEXT NOT NULL,
  trigger_group TEXT NOT NULL,
  instance_name TEXT NOT NULL,
  fired_time BIGINT NOT NULL,
  sched_time BIGINT NOT NULL,
  priority INTEGER NOT NULL,
  state TEXT NOT NULL,
  job_name TEXT,
  job_group TEXT,
  is_nonconcurrent BOOLEAN NOT NULL,
  requests_recovery BOOLEAN,
  PRIMARY KEY (sched_name, entry_id)
);

CREATE TABLE qrtz_scheduler_state (
  sched_name TEXT NOT NULL,
  instance_name TEXT NOT NULL,
  last_checkin_time BIGINT NOT NULL,
  checkin_interval BIGINT NOT NULL,
  PRIMARY KEY (sched_name, instance_name)
);

CREATE TABLE qrtz_locks (
  sched_name TEXT NOT NULL,
  lock_name TEXT NOT NULL,
  PRIMARY KEY (sched_name, lock_name)
);
