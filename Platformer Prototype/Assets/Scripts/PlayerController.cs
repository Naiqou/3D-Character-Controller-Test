using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{

    private CharacterController _controller;
    private Animator _animator;

    private Vector3 _playerVelocity;

    [Header("Ground Settings")] [SerializeField]
    private bool isGrounded;
    [SerializeField] private float groundRadius = 1;
    public Transform groundCheckPosition;

    [Space(2)] [Header("Player Movement Settings")] 
    [SerializeField]
    private float playerSpeed = 2f;
    [SerializeField]
    private float jumpHeight = 1.0f;
    [SerializeField]
    private int jumpCount = 0;

    [SerializeField]
    private bool isFalling;

    private float _gravity;

    private LayerMask _ground;
    private static readonly int Jumping = Animator.StringToHash("IsJumping");
    private static readonly int DoubleJumping = Animator.StringToHash("IsDoubleJumping");
    private static readonly int IsFalling = Animator.StringToHash("IsFalling");
    private static readonly int IsSliding = Animator.StringToHash("IsSliding");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");

    void Start()
{
    _gravity = Physics.gravity.y;
    _controller = GetComponent<CharacterController>();
    _ground = LayerMask.GetMask("Ground");
    _animator = GetComponent<Animator>();
}

    // Update is called once per frame
    void Update()
    {
        PlayerMovement();
        PlayerJump();
        PlayerGravity();
        GroundCheck();
        PlayerIsFalling();
        PlayerSlide();
    }


    void PlayerMovement()
    {
        var direction = new Vector3(0, 0, Input.GetAxis("Vertical"));
        var rotation = new Vector3(0,Input.GetAxis("Horizontal"),0);
        var moving = direction.z;
        
        if (moving < 0)
        {
            moving = Mathf.Abs(moving);
        }
        transform.Rotate(rotation);
        var move = transform.TransformDirection(direction) * playerSpeed * Time.deltaTime;
            _controller.Move(move);
            _animator.SetFloat(IsMoving, moving);
    }

    void PlayerGravity()
    {
        
        _playerVelocity.y += _gravity * Time.deltaTime;
        _controller.Move(_playerVelocity * Time.deltaTime);
    }

    void PlayerJump()
    {
        
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            _playerVelocity.y += Mathf.Sqrt(jumpHeight * -2f * _gravity);
            jumpCount++;
        }
        if (Input.GetButtonDown("Jump") && jumpCount == 1 && !isGrounded)
        {
            _playerVelocity.y += Mathf.Sqrt(jumpHeight*2 * -2f * _gravity);
            _animator.SetBool(DoubleJumping, true);
            jumpCount++;
        }


        if (_playerVelocity.y == 0)
        {
            jumpCount=0;
        }

        _animator.SetBool(Jumping, !isGrounded);

        if (isFalling || isGrounded)
        {
            _animator.SetBool(Jumping, false);
            _animator.SetBool(DoubleJumping, false);
        }

        if (jumpCount == 2 && isFalling)
        {
            _animator.SetBool(DoubleJumping, true);
            isFalling = false;
            jumpCount = 0;
        }
    }

    void PlayerSlide()
    {
        _animator.SetBool(IsSliding, Input.GetButtonDown("Fire1"));
    }

    void PlayerIsFalling()
    {
        isFalling = _playerVelocity.y < 0;
        _animator.SetBool(IsFalling, isFalling);
    }
    
    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheckPosition.position, groundRadius, _ground, QueryTriggerInteraction.Ignore);
        if (isGrounded && _playerVelocity.y < 0)
        {
            _playerVelocity.y = 0f;

        }


    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckPosition.position, groundRadius);
    }
}
