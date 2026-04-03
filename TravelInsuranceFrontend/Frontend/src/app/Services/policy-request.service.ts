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
    resubmissionCount: number;
    maxResubmissions: number;
    requestedDocTypes?: string;
    requestedAt: string;
    reviewedAt?: string;
}

export interface AgentPolicyRequestResponse extends PolicyRequestResponse {
    customerName: string;
    
    // AI Risk Analysis
    riskScore: number;
    riskLevel: string;
    riskReasoning?: string;
    countryRiskMultiplier: number;
    countryRiskLevel: string;
    aiAnalysisJson?: string;
    
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
    requestedDocTypes?: string;
}

export interface PayPolicyRequestDTO {
    policyRequestId: number;
    paymentMethod?: string;
}

export interface PremiumCalculationRequest {
    policyProductId: number;
    startDate: string;
    endDate: string;
    travellerAge: number;
    destination: string;
    memberCount: number;
}

export interface PremiumCalculationResponse {
    estimatedPremium: number;
    destinationMultiplier: number;
    ageLoading: number;
    riskLevel: string;
    riskFactor: number;
    countryFlagEmoji: string;
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

    updateRequest(id: number, formData: FormData): Observable<PolicyRequestResponse> {
        return this.http.put<PolicyRequestResponse>(`${this.apiUrl}/PolicyRequest/${id}`, formData);
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

    calculatePremium(dto: PremiumCalculationRequest): Observable<PremiumCalculationResponse> {
        return this.http.post<PremiumCalculationResponse>(`${this.apiUrl}/Policy/calculate-premium`, dto);
    }
}
