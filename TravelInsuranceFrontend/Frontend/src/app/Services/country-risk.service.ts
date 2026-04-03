import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CountryRisk {
    id: number;
    name: string;
    multiplier: number; // Changed from riskFactor (int) to multiplier (decimal)
    isActive: boolean;
    createdAt: string;
}

@Injectable({
    providedIn: 'root'
})
export class CountryRiskService {
    private http = inject(HttpClient);
    private apiUrl = `https://localhost:7161/api/admin/country-risks`;

    getAll(): Observable<CountryRisk[]> {
        return this.http.get<CountryRisk[]>(this.apiUrl);
    }

    getActive(): Observable<CountryRisk[]> {
        return this.http.get<CountryRisk[]>(`${this.apiUrl}/active`);
    }

    getById(id: number): Observable<CountryRisk> {
        return this.http.get<CountryRisk>(`${this.apiUrl}/${id}`);
    }

    create(dto: any): Observable<CountryRisk> {
        return this.http.post<CountryRisk>(this.apiUrl, dto);
    }

    update(id: number, dto: any): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${id}`, dto);
    }

    delete(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
}
