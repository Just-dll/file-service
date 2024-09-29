import { Component, Inject } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  MatDialog,
  MAT_DIALOG_DATA,
  MatDialogTitle,
  MatDialogContent,
  MatDialogRef,
} from '@angular/material/dialog';

export interface ShareFolderData {
  folderId: number;
}

@Component({
  selector: 'app-share-folder-dialog',
  templateUrl: './share-folder-dialog.component.html',
  styleUrl: './share-folder-dialog.component.scss'
})
export class ShareFolderDialogComponent {
  shareForm: FormGroup;
  permissions = [
    { name: 'Create', value: 1 },
    { name: 'Read', value: 2 },
    { name: 'Update', value: 4 },
    { name: 'Delete', value: 8 }
  ];

  constructor(
    public dialogRef: MatDialogRef<ShareFolderDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ShareFolderData,
    private fb: FormBuilder
  ) {
    // Initialize the form with custom validator
    this.shareForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      accessPermissions: [0, this.atLeastOnePermission]  // Use custom validator
    });
  }

  // Custom validator to check at least one permission is selected
  atLeastOnePermission(control: AbstractControl): { [key: string]: boolean } | null {
    return (control.value > 0) ? null : { 'atLeastOne': true };
  }

  // Function to toggle the permission flag value
  togglePermission(permissionValue: number, isChecked: boolean) {
    const currentPermissions = this.shareForm.get('accessPermissions')?.value;
    if (isChecked) {
      this.shareForm.patchValue({ accessPermissions: currentPermissions | permissionValue });
    } else {
      this.shareForm.patchValue({ accessPermissions: currentPermissions & ~permissionValue });
    }
  }

  // Close dialog without saving
  onCancel(): void {
    this.dialogRef.close();
  }

  // Submit the form
  onSubmit(): void {
    if (this.shareForm.valid) {
      this.dialogRef.close(this.shareForm.value);  // Pass the form data back to the parent
    }
  }
}