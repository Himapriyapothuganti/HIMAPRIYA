import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CountryRiskService, CountryRisk } from '../../../Services/country-risk.service';
import { Toast } from '../../admin/components/toast/toast';
import { Spinner } from '../../admin/components/spinner/spinner';
import { finalize } from 'rxjs/operators';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-country-management',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, Toast, Spinner],
  templateUrl: './country-management.component.html',
  styleUrl: './country-management.component.css'
})
export class CountryManagementComponent implements OnInit {
  private fb = inject(FormBuilder);
  private countryRiskService = inject(CountryRiskService);

  // --- SIGNALS STATE ---
  countries = signal<CountryRisk[]>([]);
  isLoading = signal<boolean>(true);
  isSubmitting = signal<boolean>(false);
  error = signal<string>('');
  
  searchTerm = signal<string>('');
  sortField = signal<string>('name');
  sortDirection = signal<'asc' | 'desc'>('asc');
  
  showAddModal = signal<boolean>(false);
  editingCountryId = signal<number | null>(null);
  countryForm: FormGroup;

  // Standardized professional destination list
  availableCountries = [
    { name: 'Afghanistan' }, { name: 'Albania' }, { name: 'Algeria' },
    { name: 'Andorra' }, { name: 'Angola' }, { name: 'Argentina' },
    { name: 'Armenia' }, { name: 'Australia' }, { name: 'Austria' },
    { name: 'Azerbaijan' }, { name: 'Bahamas' }, { name: 'Bahrain' },
    { name: 'Bangladesh' }, { name: 'Barbados' }, { name: 'Belarus' },
    { name: 'Belgium' }, { name: 'Belize' }, { name: 'Benin' },
    { name: 'Bhutan' }, { name: 'Bolivia' }, { name: 'Bosnia and Herzegovina' },
    { name: 'Botswana' }, { name: 'Brazil' }, { name: 'Brunei' },
    { name: 'Bulgaria' }, { name: 'Burkina Faso' }, { name: 'Burundi' },
    { name: 'Cambodia' }, { name: 'Cameroon' }, { name: 'Canada' },
    { name: 'Cape Verde' }, { name: 'Central African Republic' }, { name: 'Chad' },
    { name: 'Chile' }, { name: 'China' }, { name: 'Colombia' },
    { name: 'Comoros' }, { name: 'Congo' }, { name: 'Costa Rica' },
    { name: 'Croatia' }, { name: 'Cuba' }, { name: 'Cyprus' },
    { name: 'Czech Republic' }, { name: 'Denmark' }, { name: 'Djibouti' },
    { name: 'Dominica' }, { name: 'Dominican Republic' }, { name: 'Ecuador' },
    { name: 'Egypt' }, { name: 'El Salvador' }, { name: 'Equatorial Guinea' },
    { name: 'Eritrea' }, { name: 'Estonia' }, { name: 'Eswatini' },
    { name: 'Ethiopia' }, { name: 'Fiji' }, { name: 'Finland' },
    { name: 'France' }, { name: 'Gabon' }, { name: 'Gambia' },
    { name: 'Georgia' }, { name: 'Germany' }, { name: 'Ghana' },
    { name: 'Greece' }, { name: 'Grenada' }, { name: 'Guatemala' },
    { name: 'Guinea' }, { name: 'Guinea-Bissau' }, { name: 'Guyana' },
    { name: 'Haiti' }, { name: 'Honduras' }, { name: 'Hungary' },
    { name: 'Iceland' }, { name: 'India' }, { name: 'Indonesia' },
    { name: 'Iran' }, { name: 'Iraq' }, { name: 'Ireland' },
    { name: 'Israel' }, { name: 'Italy' }, { name: 'Jamaica' },
    { name: 'Japan' }, { name: 'Jordan' }, { name: 'Kazakhstan' },
    { name: 'Kenya' }, { name: 'Kiribati' }, { name: 'Korea (North)' },
    { name: 'Korea (South)' }, { name: 'Kuwait' }, { name: 'Kyrgyzstan' },
    { name: 'Laos' }, { name: 'Latvia' }, { name: 'Lebanon' },
    { name: 'Lesotho' }, { name: 'Liberia' }, { name: 'Libya' },
    { name: 'Liechtenstein' }, { name: 'Lithuania' }, { name: 'Luxembourg' },
    { name: 'Madagascar' }, { name: 'Malawi' }, { name: 'Malaysia' },
    { name: 'Maldives' }, { name: 'Mali' }, { name: 'Malta' },
    { name: 'Marshall Islands' }, { name: 'Mauritania' }, { name: 'Mauritius' },
    { name: 'Mexico' }, { name: 'Micronesia' }, { name: 'Moldova' },
    { name: 'Monaco' }, { name: 'Mongolia' }, { name: 'Montenegro' },
    { name: 'Morocco' }, { name: 'Mozambique' }, { name: 'Myanmar' },
    { name: 'Namibia' }, { name: 'Nauru' }, { name: 'Nepal' },
    { name: 'Netherlands' }, { name: 'New Zealand' }, { name: 'Nicaragua' },
    { name: 'Niger' }, { name: 'Nigeria' }, { name: 'North Macedonia' },
    { name: 'Norway' }, { name: 'Oman' }, { name: 'Pakistan' },
    { name: 'Palau' }, { name: 'Panama' }, { name: 'Papua New Guinea' },
    { name: 'Paraguay' }, { name: 'Peru' }, { name: 'Philippines' },
    { name: 'Poland' }, { name: 'Portugal' }, { name: 'Qatar' },
    { name: 'Romania' }, { name: 'Russia' }, { name: 'Rwanda' },
    { name: 'Saint Kitts and Nevis' }, { name: 'Saint Lucia' }, { name: 'Saint Vincent and the Grenadines' },
    { name: 'Samoa' }, { name: 'San Marino' }, { name: 'Sao Tome and Principe' },
    { name: 'Saudi Arabia' }, { name: 'Senegal' }, { name: 'Serbia' },
    { name: 'Seychelles' }, { name: 'Sierra Leone' }, { name: 'Singapore' },
    { name: 'Slovakia' }, { name: 'Slovenia' }, { name: 'Solomon Islands' },
    { name: 'Somalia' }, { name: 'South Africa' }, { name: 'South Sudan' },
    { name: 'Spain' }, { name: 'Sri Lanka' }, { name: 'Sudan' },
    { name: 'Suriname' }, { name: 'Sweden' }, { name: 'Switzerland' },
    { name: 'Syria' }, { name: 'Taiwan' }, { name: 'Tajikistan' },
    { name: 'Tanzania' }, { name: 'Thailand' }, { name: 'Timor-Leste' },
    { name: 'Togo' }, { name: 'Tonga' }, { name: 'Trinidad and Tobago' },
    { name: 'Tunisia' }, { name: 'Turkey' }, { name: 'Turkmenistan' },
    { name: 'Tuvalu' }, { name: 'Uganda' }, { name: 'Ukraine' },
    { name: 'United Arab Emirates' }, { name: 'United Kingdom' }, { name: 'United States' },
    { name: 'Uruguay' }, { name: 'Uzbekistan' }, { name: 'Vanuatu' },
    { name: 'Vatican City' }, { name: 'Venezuela' }, { name: 'Vietnam' },
    { name: 'Yemen' }, { name: 'Zambia' }, { name: 'Zimbabwe' }
  ];

  filteredAvailableCountries = signal<any[]>(this.availableCountries);
  pickerSearchTerm = signal<string>('');

  // --- REACTIVE COMPUTED SIGNALS ---
  computedFilteredCountries = computed(() => {
    const term = this.searchTerm().toLowerCase();
    const list = this.countries().filter(c => 
      c.name.toLowerCase().includes(term)
    );

    const field = this.sortField();
    const dir = this.sortDirection();

    return [...list].sort((a: any, b: any) => {
      const valA = a[field] ?? '';
      const valB = b[field] ?? '';
      
      if (typeof valA === 'string' && typeof valB === 'string') {
        return dir === 'asc' 
          ? valA.localeCompare(valB)
          : valB.localeCompare(valA);
      } else {
        const numA = Number(valA);
        const numB = Number(valB);
        return dir === 'asc' ? numA - numB : numB - numA;
      }
    });
  });

  constructor() {
    this.countryForm = this.fb.group({
      name: ['', [Validators.required]],
      multiplier: [1.0, [Validators.required, Validators.min(0.5), Validators.max(5.0)]], // Direct Multiplier Control
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.loadCountries();
  }

  loadCountries() {
    this.isLoading.set(true);
    this.countryRiskService.getAll().pipe(
      finalize(() => this.isLoading.set(false))
    ).subscribe({
      next: (data) => this.countries.set(data),
      error: (err) => {
        console.error('Failed to load countries:', err);
        this.error.set('Failed to load countries. Please check if the backend is running.');
      }
    });
  }

  onPickerSearch(term: string) {
    this.pickerSearchTerm.set(term);
    this.filteredAvailableCountries.set(
      this.availableCountries.filter(c => 
        c.name.toLowerCase().includes(term.toLowerCase())
      )
    );
  }

  selectFromPicker(country: any) {
    const exists = this.countries().some(c => c.name.toLowerCase() === country.name.toLowerCase());
    if (exists && !this.editingCountryId()) {
      this.error.set(`Country '${country.name}' is already in the management list.`);
      return;
    }

    this.countryForm.patchValue({
      name: country.name
    });
    this.pickerSearchTerm.set('');
    this.filteredAvailableCountries.set(this.availableCountries);
    this.error.set('');
  }

  toggleAddModal() {
    const nextValue = !this.showAddModal();
    this.showAddModal.set(nextValue);
    if (!nextValue) {
      this.resetForm();
    }
  }

  editCountry(country: CountryRisk) {
    this.editingCountryId.set(country.id);
    this.countryForm.patchValue({
      name: country.name,
      multiplier: country.multiplier,
      isActive: country.isActive
    });
    this.showAddModal.set(true);
  }

  resetForm() {
    this.editingCountryId.set(null);
    this.countryForm.reset({ multiplier: 1.0, isActive: true });
    this.error.set('');
    this.pickerSearchTerm.set('');
  }

  onSubmit() {
    if (this.countryForm.invalid) return;

    this.isSubmitting.set(true);
    const id = this.editingCountryId();
    
    const request: Observable<any> = id 
      ? this.countryRiskService.update(id, this.countryForm.value)
      : this.countryRiskService.create(this.countryForm.value);

    request.pipe(
      finalize(() => this.isSubmitting.set(false))
    ).subscribe({
      next: () => {
        this.loadCountries();
        this.toggleAddModal();
      },
      error: (err: any) => {
        this.error.set(err.error?.message || 'Failed to process request.');
      }
    });
  }

  toggleStatus(country: CountryRisk) {
    const updated = { multiplier: country.multiplier, isActive: !country.isActive };
    this.countryRiskService.update(country.id, updated).subscribe({
      next: () => this.loadCountries(),
      error: () => this.error.set('Failed to update status.')
    });
  }

  deleteCountry(id: number) {
    if (confirm('Are you sure you want to remove this country from the risk system?')) {
      this.countryRiskService.delete(id).subscribe({
        next: () => this.loadCountries(),
        error: (err: any) => this.error.set(err.error?.message || 'Failed to delete country.')
      });
    }
  }

  setSort(field: string) {
    if (this.sortField() === field) {
      this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortField.set(field);
      this.sortDirection.set('asc');
    }
  }

  closeToast() {
    this.error.set('');
  }
}
