
export class AccountInfo implements MedEasy.DTO.Resource<string> {

    username: string;
    email: string;
    locked: boolean;
    name: string;
    tenantId: string;
    claims: Array<{ type: string, value: string }>;
    id: string
    updatedDate?: Date;
    createdDate: Date;

}