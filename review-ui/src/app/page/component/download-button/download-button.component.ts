import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-download-button',
  imports: [CommonModule],
  templateUrl: './download-button.component.html',
  styleUrls: ['./download-button.component.css']
})
export class DownloadButtonComponent {
  showDownloadMenu = false;

  toggleDownloadMenu() {
    this.showDownloadMenu = !this.showDownloadMenu;
  }

  downloadExcel() {
    console.log('Downloading Excel...');
    // TODO: Implement Excel download
    this.showDownloadMenu = false;
  }

  downloadPDF() {
    console.log('Downloading PDF...');
    // TODO: Implement PDF download
    this.showDownloadMenu = false;
  }

  downloadWord() {
    console.log('Downloading Word...');
    // TODO: Implement Word download
    this.showDownloadMenu = false;
  }
}
