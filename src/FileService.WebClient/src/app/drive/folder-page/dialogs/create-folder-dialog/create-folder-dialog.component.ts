import { Component, Inject } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  MatDialog,
  MAT_DIALOG_DATA,
  MatDialogTitle,
  MatDialogContent,
  MatDialogRef,
} from '@angular/material/dialog';

@Component({
  selector: 'app-create-folder-dialog',
  templateUrl: './create-folder-dialog.component.html',
  styleUrls: ['./create-folder-dialog.component.scss']
})
export class CreateFolderDialogComponent {
  createFolderForm: FormGroup;

  constructor(
    public dialogRef: MatDialogRef<CreateFolderDialogComponent>,
    private fb: FormBuilder
  ) {
    // Initialize the form with folder name validation
    this.createFolderForm = this.fb.group({
      folderName: ['', [
        Validators.required, 
        Validators.minLength(3), 
        this.forbiddenSymbolsValidator  // Custom validator
      ]]
    });
  }

  // Custom validator to check for forbidden symbols in folder name
  forbiddenSymbolsValidator(control: AbstractControl): { [key: string]: boolean } | null {
    const forbiddenSymbols = /[<>:"\/\\|?*\x00-\x1F]/;  // Common forbidden characters in folder names
    return forbiddenSymbols.test(control.value) ? { 'invalidSymbols': true } : null;
  }

  // Close dialog without saving
  onCancel(): void {
    this.dialogRef.close();
  }

  // Submit the form
  onSubmit(): void {
    if (this.createFolderForm.valid) {
      this.dialogRef.close(this.createFolderForm.value);  // Pass the form data back to the parent
    }
  }
}
