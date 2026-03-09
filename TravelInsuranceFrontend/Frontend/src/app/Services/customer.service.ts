import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({
    providedIn: 'root'  // Available across the entire app — no need to import per component
})
export class CustomerService {
    // The base URL of our ASP.NET Core backend
    private apiUrl = 'https://localhost:7161/api';

    constructor(private http: HttpClient) { }

    // ── POLICIES ─────────────────────────────────────
    getPolicyProducts(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/Policy/products`);
    }

    getPolicyDetails(policyId: number | string): Observable<any> {
        return this.http.get<any>(`${this.apiUrl}/Policy/${policyId}`);
    }

    getMyPolicies(): Observable<any[]> {
        return this.http.get<any[]>(`${this.apiUrl}/Policy/my-policies`);
    }

    purchasePolicy(purchaseData: any): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/Policy/purchase`, purchaseData);
    }

    payPremium(paymentData: any): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/Policy/pay`, paymentData);
    }

    // ── CLAIMS ───────────────────────────────────────
    getMyClaims(): Observable<any[]> {
        // GET request to fetch all claims submitted by the logged-in customer
        return this.http.get<any[]>(`${this.apiUrl}/Claim/my-claims`);
    }

    submitClaim(formData: FormData): Observable<any> {
        // POST request — sends form fields + uploaded documents to the backend
        // FormData is used because we are uploading files (multipart/form-data)
        return this.http.post<any>(`${this.apiUrl}/Claim/submit`, formData);
    }
}
