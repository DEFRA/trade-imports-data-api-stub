# Scenario Generator

All stub scenarios originate from input data on our system boundary, which is consumed and then added to the data API.

Each step has been defined in tests so it's relatively easy to run.

## Dependencies

Clone https://github.com/DEFRA/btms-local-environment and run locally via the README guidance.

## Import data

See tests within the `Load` folder.

Given an empty Mongo DB, import the data as follows:

### 01-IPAFFS

1. Import CHEDs via `ImportPreNotificationTests`.

### 02-CDS

1. Import clearance requests via `ClearanceRequestTests`.
2. Import finalisations via the `FinalisationTests`.

### 03-GMR

1. Import GMRs via 'GmrTests'.

## Update stubs

See tests within the `Stubs` folder.

### 01-Save

1. Save new stubs via `SaveTests`.

### 02-Move

1. Move new stubs via `MoveTests`.
2. Assess the diffs and commit if they are what you expect.