using Api.Controllers;
using Domain.Dtos;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;

namespace API.Controllers;
[Route("/API/V1/[controller]")]
public class AuthenticationController : BaseApiController
{
    
    private readonly IAuthorizationService _authorizationService;
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationController(IAuthorizationService authorizationService, IAuthenticationService 
        authService)
    {
        _authorizationService = authorizationService;
        _authenticationService = authService;
    }
    
    [HttpPost("init-handshake")]
    public IActionResult InitiateSignIn([FromBody] AuthenticationHandshake request)
    {
        try 
        {
            var host = Request.Host.Host;
            var uniqueMessage = _authenticationService.GetMessage(host, request);
            return Ok(uniqueMessage);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new {
                Message = "An error occurred while initiating sign-in.",
                Error = ex.Message 
            });
        }
    }

    [HttpPost("finish-handshake")]
    public IActionResult VerifySignature([FromBody] AuthRequest request)
    {
        try
        {
            var host = Request.Host.Host;
            var isSignatureValid = _authenticationService.VerifyMessage(request, host);
            if (!isSignatureValid)
                return BadRequest(new { VerificationStatus = "Signature is not valid." });

            // _childService.DeviceExists(request.PublicKey);
            var childToken = _authorizationService.IssueChildToken(Guid.Empty);
            return Ok(new { Token = childToken, Expires = DateTime.UtcNow.AddHours(1) });
        }
        catch (Exception ex)
        {
            return StatusCode(
                500, new
                {
                    Message = "An error occurred while verifying the signature or token has expired."
                }
            );
        }
    }
}
