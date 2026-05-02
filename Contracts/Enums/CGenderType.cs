namespace HTT.BlazorWasm.App.Contracts
{
    public enum CGenderType
    {
        [HTTMetadata(Name = "Common.Gender.None", Icon = "bi bi-gender-neuter")]
        None = 0,

        [HTTMetadata(Name = "Common.Gender.Male", Icon = "bi bi-gender-male")]
        Male = 1,

        [HTTMetadata(Name = "Common.Gender.Female", Icon = "bi bi-gender-female")]
        Female = 2,

        [HTTMetadata(Name = "Common.Gender.Other", Icon = "bi bi-gender-transgender")]
        Other = 3
    }
}