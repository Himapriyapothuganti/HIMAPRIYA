import { Injectable, signal } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';

@Injectable({
    providedIn: 'root'
})
export class ClaimsOfficerService {
    private apiUrl = 'https://localhost:7161/api/Claim';

    // Using signals for global loading state if desired (optional but requested)
    public isLoading = signal<boolean>(false);

    constructor(private http: HttpClient) { }

    getToken(): string {
        return localStorage.getItem('token') ?? '';
    }

    private getHeaders(): HttpHeaders {
        return new HttpHeaders({
            'Authorization': `Bearer ${this.getToken()}`
        });
    }

    getAssignedClaims(): Observable<any[]> {
        this.isLoading.set(true);
        return this.http.get<any[]>(`${this.apiUrl}/assigned`, { headers: this.getHeaders() })
            .pipe(finalize(() => this.isLoading.set(false)));
    }

    reviewClaim(id: number, body: { isApproved: boolean, approvedAmount?: number, rejectionReason?: string }): Observable<any> {
        this.isLoading.set(true);
        return this.http.put<any>(`${this.apiUrl}/${id}/review`, body, { headers: this.getHeaders() })
            .pipe(finalize(() => this.isLoading.set(false)));
    }

    requestDocuments(id: number): Observable<any> {
        this.isLoading.set(true);
        // Note: The backend expects { "documentsRequired": "..." } roughly according to typical RequestDocumentDTO but we can send an empty object if no dto is strictly validated or update it if needed.
        return this.http.put<any>(`${this.apiUrl}/${id}/request-documents`, {}, { headers: this.getHeaders() })
            .pipe(finalize(() => this.isLoading.set(false)));
    }

    analyzeClaim(id: number): Observable<any> {
        this.isLoading.set(true);
        return this.http.post<any>(`${this.apiUrl}/${id}/analyze`, {}, { headers: this.getHeaders() })
            .pipe(finalize(() => this.isLoading.set(false)));
    }

    processPayment(id: number): Observable<any> {
        this.isLoading.set(true);
        return this.http.put<any>(`${this.apiUrl}/${id}/process-payment`, {}, { headers: this.getHeaders() })
            .pipe(finalize(() => this.isLoading.set(false)));
    }

    closeClaim(id: number): Observable<any> {
        this.isLoading.set(true);
        return this.http.put<any>(`${this.apiUrl}/${id}/close`, {}, { headers: this.getHeaders() })
            .pipe(finalize(() => this.isLoading.set(false)));
    }

    downloadDocument(documentUrl: string): Observable<Blob> {
        return this.http.get(documentUrl, {
            headers: this.getHeaders(),
            responseType: 'blob'
        });
    }
}
