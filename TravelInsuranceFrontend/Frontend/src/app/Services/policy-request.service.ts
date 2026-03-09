import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface PolicyRequestResponse {
    policyRequestId: number;
    policyProductId: number;
    policyName: string;
    policyType: string;
    planTier: string;
    destination: string;
    startDate: string;
    endDate: string;
    travellerName: string;
    travellerAge: number;
    kycType: string;
    kycNumber: string;
    passportNumber: string;
    status: string;
    rejectionReason?: string;
    calculatedPremium: number;
    requestedAt: string;
    reviewedAt?: string;
}

export interface AgentPolicyRequestResponse extends PolicyRequestResponse {
    customerName: string;
    riskScore: number;
    riskAgeScore: number;
    riskDestinationScore: number;
    riskDurationScore: number;
    riskTierScore: number;
    riskLevel: string;
    agentNotes?: string;
    documents: Array<{
        policyRequestDocumentId: number;
        fileName: string;
        fileType: string;
        documentType: string;
        fileSize: number;
        uploadedAt: string;
        fileUrl: string;
    }>;
}

export interface ReviewPolicyRequestDTO {
    status: string;
    rejectionReason?: string;
    agentNotes?: string;
}

export interface PayPolicyRequestDTO {
    policyRequestId: number;
    paymentMethod?: string;
}

@Injectable({
    providedIn: 'root'
})
export class PolicyRequestService {
    private http = inject(HttpClient);
    private apiUrl = `https://localhost:7161/api`; // e.g. https://localhost:7161/api

    // ── CUSTOMER ENDPOINTS ────────────────────────────────────────

    submitRequest(formData: FormData): Observable<PolicyRequestResponse> {
        return this.http.post<PolicyRequestResponse>(`${this.apiUrl}/PolicyRequest`, formData);
    }

    getMyRequests(): Observable<PolicyRequestResponse[]> {
        return this.http.get<PolicyRequestResponse[]>(`${this.apiUrl}/PolicyRequest/my-requests`);
    }

    payRequest(dto: PayPolicyRequestDTO): Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/Policy/pay-request`, dto);
    }

    // ── AGENT ENDPOINTS ───────────────────────────────────────────

    getAgentRequests(): Observable<AgentPolicyRequestResponse[]> {
        return this.http.get<AgentPolicyRequestResponse[]>(`${this.apiUrl}/Agent/policy-requests`);
    }

    getAgentRequestById(id: number): Observable<AgentPolicyRequestResponse> {
        return this.http.get<AgentPolicyRequestResponse>(`${this.apiUrl}/Agent/policy-requests/${id}`);
    }

    reviewRequest(id: number, dto: ReviewPolicyRequestDTO): Observable<AgentPolicyRequestResponse> {
        return this.http.put<AgentPolicyRequestResponse>(`${this.apiUrl}/Agent/policy-requests/${id}/review`, dto);
    }

    downloadDocument(documentUrl: string): Observable<Blob> {
        return this.http.get(documentUrl, { responseType: 'blob' });
    }
}
